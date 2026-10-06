using MySite.Domain.Content;

namespace MySite.Application.Content;

/// <summary>
/// Reads the site's public content. It speaks in domain types only; where the content is stored is
/// not this interface's business.
/// </summary>
public interface IContentRepository
{
    Task<OwnerProfile?> GetProfileAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(CancellationToken cancellationToken);
}
