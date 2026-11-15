namespace MySite.Infrastructure.Postgres.NegativeFixture;

/// <summary>
/// Deliberately depends on Web layer via typeof.
/// </summary>
public static class LeakyAdapter
{
    public static string ExtensionTypeName() => typeof(MySite.Web.DependencyInjectionExtension).FullName!;
}
