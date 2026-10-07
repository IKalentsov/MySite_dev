using MySite.Contracts.Content;
using MySite.Domain.Content;

namespace MySite.Application.Content;

/// <summary>
/// Decides what the public page contains: which projects are shown, which text they show, and
/// where the next page of projects begins. It knows nothing about HTTP.
/// </summary>
public sealed class ContentService : IContentService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IContentRepository _content;

    /// <summary>Creates the use case.</summary>
    /// <param name="content">Reads the stored content.</param>
    public ContentService(IContentRepository content)
    {
        _content = content;
    }

    /// <inheritdoc />
    public async Task<HomeResponse> GetHomeAsync(
        Locale locale, ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var profile = await GetProfileAsync(locale, cancellationToken);
        var projects = await GetProjectsAsync(locale, cursor, pageSize, cancellationToken);

        return new HomeResponse(profile, projects);
    }

    /// <inheritdoc />
    public async Task<OwnerProfileResponse?> GetProfileAsync(
        Locale locale, CancellationToken cancellationToken)
    {
        var profile = await _content.GetProfileAsync(cancellationToken);
        var translation = profile?.TranslationFor(locale);

        return translation is null
            ? null
            : new OwnerProfileResponse(translation.Locale.Value, translation.Headline, translation.About);
    }

    /// <inheritdoc />
    public async Task<PaginatedProjectsResponse> GetProjectsAsync(
        Locale locale, ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var size = ClampPageSize(pageSize);

        // The repository returns one row more than the page: that row says whether a further page
        // exists, without a second query and without counting the table.
        var projects = await _content.GetPublishedProjectsAsync(cursor, size, cancellationToken);

        var hasMore = projects.Count > size;
        var page = hasMore ? projects.Take(size).ToList() : [.. projects];

        // A project with no text in the requested locale and none in the default one is not shown:
        // there would be nothing to render.
        var items = page
            .Select(project => MapProject(project, locale))
            .OfType<ProjectResponse>()
            .ToList();

        // The cursor is the last row read, not the last row shown: a hidden project must not send
        // the next page back over rows the caller has already received.
        var last = page.Count > 0 ? page[^1] : null;
        var nextCursor = hasMore && last is not null
            ? new ProjectCursor(last.Year, last.Id).ToString()
            : null;

        return new PaginatedProjectsResponse(items, nextCursor, hasMore);
    }

    private static ProjectResponse? MapProject(Project project, Locale locale)
    {
        var translation = project.TranslationFor(locale);

        return translation is null
            ? null
            : new ProjectResponse(
                translation.Title,
                translation.Summary,
                project.Stack
                    .OrderBy(item => item.Position)
                    .Select(item => item.Name)
                    .ToList(),
                project.Link,
                project.Year);
    }

    private static int ClampPageSize(int value)
    {
        if (value <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(value, MaxPageSize);
    }
}
