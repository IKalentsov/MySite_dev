namespace MySite.Contracts.NegativeFixture;

/// <summary>
/// Deliberately depends on Application layer via typeof.
/// </summary>
public static class LeakyDto
{
    public static string ServiceTypeName() => typeof(MySite.Application.Content.IContentService).FullName!;
}
