namespace MySite.Application.Content.Seeding;

/// <summary>Describes the result of a seed operation.</summary>
public enum SeedContentStatus
{
    /// <summary>The content was written successfully.</summary>
    Seeded,

    /// <summary>The input was invalid and nothing was written.</summary>
    Invalid
}
