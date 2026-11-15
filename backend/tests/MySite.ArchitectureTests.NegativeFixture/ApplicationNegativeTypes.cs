namespace MySite.Application.NegativeFixture;

/// <summary>
/// Deliberately depends on Infrastructure layer via typeof.
/// </summary>
public static class LeakyUseCase
{
    public static string DbTypeName() => typeof(MySite.Infrastructure.Postgres.Persistence.MySiteDbContext).FullName!;
}
