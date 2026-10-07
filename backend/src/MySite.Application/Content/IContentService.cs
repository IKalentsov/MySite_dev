using MySite.Contracts.Content;
using MySite.Domain.Content;

namespace MySite.Application.Content;

/// <summary>
/// The content use cases behind the public API. They answer in the API's response shapes, so the
/// controller only resolves the request and turns the answer into an HTTP response.
/// </summary>
public interface IContentService
{
    /// <summary>Returns the combined home page content: profile and published projects.</summary>
    /// <param name="locale">The locale to answer in.</param>
    /// <param name="cursor">The projects page to continue from, or null for the first page.</param>
    /// <param name="pageSize">The requested number of projects per page.</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <returns>The home page content.</returns>
    Task<HomeResponse> GetHomeAsync(
        Locale locale, ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the owner's "about me" text, or null when it has not been written yet.</summary>
    /// <param name="locale">The locale to answer in.</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <returns>The profile text, or null.</returns>
    Task<OwnerProfileResponse?> GetProfileAsync(Locale locale, CancellationToken cancellationToken);

    /// <summary>Returns the published projects with keyset pagination.</summary>
    /// <param name="locale">The locale to answer in.</param>
    /// <param name="cursor">The page to continue from, or null for the first page.</param>
    /// <param name="pageSize">The requested number of projects per page.</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <returns>The projects of this page and where the next page continues.</returns>
    Task<PaginatedProjectsResponse> GetProjectsAsync(
        Locale locale, ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken);
}
