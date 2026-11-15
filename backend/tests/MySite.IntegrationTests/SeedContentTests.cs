using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySite.Application;
using MySite.Application.Content.Seeding;
using MySite.Domain.Content;
using MySite.Infrastructure.Postgres;
using Xunit;

namespace MySite.IntegrationTests;

/// <summary>
/// The seed path against a real PostgreSQL container: the service and the writer together, with
/// the production DI composition and the project's own migration, no in-memory substitute.
/// </summary>
[Collection(PostgresCollectionDefinition.Name)]
public sealed class SeedContentTests : IAsyncLifetime
{
    private const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";

    private readonly PostgresContainerFixture _fixture;

    public SeedContentTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task FirstRunCreatesTheProfileAndTheProjects()
    {
        var result = await SeedAsync(SeedFiles.TwoProjects());

        Assert.Equal(SeedContentStatus.Seeded, result.Status);
        Assert.Empty(result.Errors);

        var outcome = result.Outcome!;
        Assert.True(outcome.ProfileCreated);
        Assert.False(outcome.ProfileUpdated);
        Assert.Equal(2, outcome.ProjectsCreated);
        Assert.Equal(0, outcome.ProjectsUpdated);
        Assert.Equal(0, outcome.ProjectsUnchanged);
        Assert.Empty(outcome.AbsentProjectLinks);

        var snapshot = await SnapshotAsync();
        Assert.Equal(1, snapshot.Profiles);
        Assert.Equal(2, snapshot.ProfileTranslations);
        Assert.Equal(2, snapshot.Projects);
        Assert.Equal(3, snapshot.ProjectTranslations);
        Assert.Equal(3, snapshot.StackItems);
    }

    [Fact]
    public async Task SecondRunChangesNothing()
    {
        var json = SeedFiles.TwoProjects();
        await SeedAsync(json);
        var before = await SnapshotAsync();

        var result = await SeedAsync(json);
        var after = await SnapshotAsync();

        Assert.Equal(before, after);

        var outcome = result.Outcome!;
        Assert.False(outcome.ProfileCreated);
        Assert.False(outcome.ProfileUpdated);
        Assert.Equal(0, outcome.ProjectsCreated);
        Assert.Equal(0, outcome.ProjectsUpdated);
        Assert.Equal(2, outcome.ProjectsUnchanged);
        Assert.Empty(outcome.AbsentProjectLinks);
    }

    [Fact]
    public async Task AddedLocaleIsWrittenAndRemovedLocaleIsDeleted()
    {
        await SeedAsync(SeedFiles.OneProject(includeEnglish: false));
        Assert.Equal(1, (await SnapshotAsync()).ProjectTranslations);

        var added = await SeedAsync(SeedFiles.OneProject(includeEnglish: true));
        Assert.Equal(1, added.Outcome!.ProjectsUpdated);
        Assert.Equal(2, (await SnapshotAsync()).ProjectTranslations);

        var english = (await ReadProjectAsync(SeedFiles.FirstLink)).TranslationFor(Locale.En);
        Assert.NotNull(english);
        Assert.Equal("First project", english.Title);

        var removed = await SeedAsync(SeedFiles.OneProject(includeEnglish: false));
        Assert.Equal(1, removed.Outcome!.ProjectsUpdated);
        Assert.Equal(1, (await SnapshotAsync()).ProjectTranslations);
        var afterRemoval = await ReadProjectAsync(SeedFiles.FirstLink);
        Assert.DoesNotContain(afterRemoval.Translations, translation => translation.Locale.Equals(Locale.En));
        var fallback = afterRemoval.TranslationFor(Locale.En);
        Assert.NotNull(fallback);
        Assert.Equal(Locale.Ru, fallback.Locale);
    }

    [Fact]
    public async Task StackGrowsAndShrinks()
    {
        await SeedAsync(SeedFiles.OneProjectWithStack("a", "b"));
        Assert.Equal(2, (await SnapshotAsync()).StackItems);

        var grown = await SeedAsync(SeedFiles.OneProjectWithStack("a", "b", "c", "d"));
        Assert.Equal(1, grown.Outcome!.ProjectsUpdated);
        Assert.Equal(4, (await SnapshotAsync()).StackItems);

        var grownProject = await ReadProjectAsync(SeedFiles.FirstLink);
        string[] expectedStack = ["a", "b", "c", "d"];
        string[] grownNames = [.. grownProject.Stack.OrderBy(item => item.Position).Select(item => item.Name)];
        Assert.Equal(expectedStack, grownNames);

        var shrunk = await SeedAsync(SeedFiles.OneProjectWithStack("b"));
        Assert.Equal(1, shrunk.Outcome!.ProjectsUpdated);
        Assert.Equal(1, (await SnapshotAsync()).StackItems);

        var onlyItem = Assert.Single((await ReadProjectAsync(SeedFiles.FirstLink)).Stack);
        Assert.Equal("b", onlyItem.Name);
        Assert.Equal(0, onlyItem.Position);
    }

    [Fact]
    public async Task ProjectMissingFromTheFileIsReportedAndKept()
    {
        await SeedAsync(SeedFiles.TwoProjects());

        var result = await SeedAsync(SeedFiles.OneProject(includeEnglish: true));
        var outcome = result.Outcome!;

        Assert.Equal(1, outcome.ProjectsUnchanged);
        var absent = Assert.Single(outcome.AbsentProjectLinks);
        Assert.Equal(SeedFiles.SecondLink, absent);

        var snapshot = await SnapshotAsync();
        Assert.Equal(2, snapshot.Projects);
        Assert.Equal(3, snapshot.ProjectTranslations);
    }

    private async Task<SeedContentResult> SeedAsync(string json)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [ConnectionStringKey] = _fixture.ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService<ISeedContentService>();

        return await service.SeedAsync(json, TestContext.Current.CancellationToken);
    }

    private async Task<DatabaseSnapshot> SnapshotAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var context = _fixture.CreateContext();

        return new DatabaseSnapshot(
            await context.OwnerProfiles.CountAsync(cancellationToken),
            await context.Set<OwnerProfileTranslation>().CountAsync(cancellationToken),
            await context.Projects.CountAsync(cancellationToken),
            await context.Set<ProjectTranslation>().CountAsync(cancellationToken),
            await context.Set<ProjectStackItem>().CountAsync(cancellationToken),
            await context.OwnerProfiles
                .Select(profile => profile.UpdatedAtUtc)
                .SingleAsync(cancellationToken));
    }

    private async Task<Project> ReadProjectAsync(string link)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var context = _fixture.CreateContext();

        return await context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .Include(project => project.Stack)
            .SingleAsync(project => project.Link == link, cancellationToken);
    }

    private sealed record DatabaseSnapshot(
        int Profiles,
        int ProfileTranslations,
        int Projects,
        int ProjectTranslations,
        int StackItems,
        DateTime ProfileUpdatedAtUtc);
}
