using Microsoft.EntityFrameworkCore;
using MySite.Infrastructure.Postgres.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace MySite.IntegrationTests;

/// <summary>
/// One PostgreSQL container for the whole integration assembly, migrated once. Every test empties
/// the tables first, so no test depends on another one's leftovers or on execution order.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private const string Image = "postgres:18.6-alpine";
    private const string TestDatabase = "mysite_tests";
    private const string TestUser = "postgres";
    private const string TestPassword = "postgres";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithDatabase(TestDatabase)
        .WithUsername(TestUser)
        .WithPassword(TestPassword)
        .Build();

    /// <summary>The connection string of the running container.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    /// <summary>Creates a context over the container's database.</summary>
    public MySiteDbContext CreateContext() => new(
        new DbContextOptionsBuilder<MySiteDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    /// <summary>Deletes every row, so each test starts from an empty database.</summary>
    public async Task ResetAsync()
    {
        await using var context = CreateContext();

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE project_stack_items, project_translations, projects, owner_profile_translations, owner_profiles RESTART IDENTITY CASCADE",
            CancellationToken.None);
    }
}
