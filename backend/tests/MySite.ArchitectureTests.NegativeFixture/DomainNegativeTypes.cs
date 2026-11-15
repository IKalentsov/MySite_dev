namespace MySite.Domain.NegativeFixture;

/// <summary>
/// Deliberately depends on Application layer via typeof.
/// </summary>
public static class LeakyDomainType
{
    public static string ApplicationTypeName() => typeof(MySite.Application.Content.IContentService).FullName!;
}

/// <summary>
/// Violates the interface naming convention (no I-prefix).
/// </summary>
public interface NotPrefixed { }

/// <summary>
/// Violates the async-void rule.
/// </summary>
public static class FireAndForget
{
    public static async void Run() => await Task.Delay(1);
}
