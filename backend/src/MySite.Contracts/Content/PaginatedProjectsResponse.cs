namespace MySite.Contracts.Content;

/// <summary>The published projects with keyset pagination metadata.</summary>
/// <param name="Items">The project items for this page.</param>
/// <param name="NextCursor">A cursor to fetch the next page, or null when this is the last page.</param>
/// <param name="HasMore">Whether there are more pages after this one.</param>
public sealed record PaginatedProjectsResponse(
    IReadOnlyList<ProjectResponse> Items,
    string? NextCursor,
    bool HasMore);
