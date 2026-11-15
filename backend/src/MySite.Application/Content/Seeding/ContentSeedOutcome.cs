namespace MySite.Application.Content.Seeding;

/// <summary>Describes what the seed operation changed in storage.</summary>
public sealed record ContentSeedOutcome(
    bool ProfileCreated,
    bool ProfileUpdated,
    int ProjectsCreated,
    int ProjectsUpdated,
    int ProjectsUnchanged,
    IReadOnlyList<string> AbsentProjectLinks);
