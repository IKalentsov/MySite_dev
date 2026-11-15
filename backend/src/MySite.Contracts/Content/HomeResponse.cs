namespace MySite.Contracts.Content;

/// <summary>
/// The combined home page response: owner profile and published projects.
/// </summary>
/// <param name="Profile">The owner's resolved profile, or null when not yet written.</param>
/// <param name="Projects">The published projects with keyset pagination metadata.</param>
public sealed record HomeResponse(
    OwnerProfileResponse? Profile,
    PaginatedProjectsResponse Projects);
