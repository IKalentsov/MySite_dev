using MySite.Domain.Content;
using Xunit;

namespace MySite.UnitTests.Domain.Content;

public sealed class ProjectTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly string[] StackA_B_C = ["a", "b", "c"];
    private static readonly string[] Stack_A_C = ["a", "c"];
    private static readonly string[] Stack_X_Y_Z_W = ["x", "y", "z", "w"];

    [Fact]
    public void ReplaceStackConvergesByPosition()
    {
        var project = Project.Create(
            Guid.NewGuid(), "https://example.com/p", 2026, 1, true, UtcNow, UtcNow);

        Assert.Empty(project.Stack);

        project.ReplaceStack(StackA_B_C);
        Assert.Equal(3, project.Stack.Count);
        string[] names1 = [.. project.Stack.OrderBy(x => x.Position).Select(x => x.Name)];
        Assert.True(StackA_B_C.SequenceEqual(names1, StringComparer.Ordinal));

        project.ReplaceStack(Stack_A_C);
        Assert.Equal(2, project.Stack.Count);
        string[] names2 = [.. project.Stack.OrderBy(x => x.Position).Select(x => x.Name)];
        Assert.True(Stack_A_C.SequenceEqual(names2, StringComparer.Ordinal));

        project.ReplaceStack(Stack_X_Y_Z_W);
        Assert.Equal(4, project.Stack.Count);
        string[] names3 = [.. project.Stack.OrderBy(x => x.Position).Select(x => x.Name)];
        Assert.True(Stack_X_Y_Z_W.SequenceEqual(names3, StringComparer.Ordinal));
    }

    [Fact]
    public void ReplaceTextsUpdatesInPlaceAndDropsRemovedLocale()
    {
        var project = Project.Create(
            Guid.NewGuid(), "https://example.com/p", 2026, 1, true, UtcNow, UtcNow);

        project.ReplaceTexts(new[]
        {
            new ProjectText(Locale.Ru, "ru title", "ru summary"),
            new ProjectText(Locale.En, "en title", "en summary")
        });

        Assert.Equal(2, project.Translations.Count);

        var ruTranslation = project.Translations.First(t => t.Locale.Equals(Locale.Ru));
        var expectedId = ruTranslation.Id;

        project.ReplaceTexts(new[]
        {
            new ProjectText(Locale.Ru, "new title", "new summary")
        });

        Assert.Single(project.Translations);
        var remaining = project.Translations.First();
        Assert.True(remaining.Locale.Equals(Locale.Ru));
        Assert.Equal("new title", remaining.Title);
        Assert.Equal("new summary", remaining.Summary);
        Assert.Equal(expectedId, remaining.Id);
    }

    [Fact]
    public void ReplaceTextsAddsNewLocale()
    {
        var project = Project.Create(
            Guid.NewGuid(), "https://example.com/p", 2026, 1, true, UtcNow, UtcNow);

        project.ReplaceTexts(new[]
        {
            new ProjectText(Locale.Ru, "ru title", "ru summary")
        });
        Assert.Single(project.Translations);

        project.ReplaceTexts(new[]
        {
            new ProjectText(Locale.Ru, "ru title", "ru summary"),
            new ProjectText(Locale.En, "en title", "en summary")
        });

        Assert.Equal(2, project.Translations.Count);
        var enTranslation = project.Translations.First(t => t.Locale.Equals(Locale.En));
        Assert.Equal("en title", enTranslation.Title);
    }
}
