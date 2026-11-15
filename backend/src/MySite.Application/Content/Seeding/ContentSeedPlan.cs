using MySite.Domain.Content;

namespace MySite.Application.Content.Seeding;

/// <summary>The profile text to write during a seed operation.</summary>
public sealed record ProfileSeed(IReadOnlyList<ProfileText> Texts);

/// <summary>A single project's data to write during a seed operation.</summary>
public sealed record ProjectSeed(
    string Link,
    int Year,
    int SortOrder,
    bool IsPublished,
    IReadOnlyList<ProjectText> Texts,
    IReadOnlyList<string> Stack);

/// <summary>The full plan derived from a validated content file.</summary>
public sealed record ContentSeedPlan(ProfileSeed Profile, IReadOnlyList<ProjectSeed> Projects);
