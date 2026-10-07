using MySite.Domain.Content;

namespace MySite.Application.Content;

/// <summary>
/// Reads the site's public content. It speaks in domain types only; where the content is stored is
/// not this interface's business.
/// </summary>
public interface IContentRepository
{
    /// <summary>Reads the owner's profile, or null when it has not been written yet.</summary>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    Task<OwnerProfile?> GetProfileAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the published projects in keyset order — the most recent year first, then the greatest
    /// identifier — starting after <paramref name="cursor"/> when one is given.
    /// </summary>
    /// <param name="cursor">The last row the caller has seen, or null to start from the first.</param>
    /// <param name="pageSize">The number of rows the caller wants to show.</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <returns>
    /// Up to <paramref name="pageSize"/> + 1 rows. The extra row means a further page exists and is
    /// the caller's signal that it does; it is not meant to be shown.
    /// </returns>
    Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(
        ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken);
}
