using Microsoft.AspNetCore.Mvc;
using MySite.Application.Content;
using MySite.Contracts.Content;
using MySite.Domain.Content;

namespace MySite.Web.Controllers;

/// <summary>The public content of the site's single page.</summary>
[ApiController]
[Route("api/v1")]
public sealed class ContentController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IContentRepository _content;

    /// <summary>Creates the controller.</summary>
    /// <param name="content">Reads the site's published content.</param>
    public ContentController(IContentRepository content)
    {
        _content = content;
    }

    /// <summary>Returns the combined home page content: profile and projects.</summary>
    /// <param name="locale">
    /// The locale to answer in: <c>ru</c> or <c>en</c>. Defaults to <c>ru</c>.
    /// </param>
    /// <param name="cursor">Keyset cursor for projects pagination (format: <c>{year}:{id}</c>).</param>
    /// <param name="pageSize">Number of projects per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <response code="200">The home page content.</response>
    /// <response code="400">Invalid locale or pagination parameters.</response>
    [HttpGet("home")]
    [ProducesResponseType<HomeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HomeResponse>> GetHome(
        [FromQuery] string? locale,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryResolveLocale(locale, out var resolved))
        {
            return UnknownLocale(locale);
        }

        pageSize = ClampPageSize(pageSize);

        var profile = await BuildProfileAsync(resolved, cancellationToken);
        var projects = await BuildPaginatedProjectsAsync(resolved, cursor, pageSize, cancellationToken);

        return Ok(new HomeResponse(profile, projects));
    }

    /// <summary>Returns the owner's "about me" text.</summary>
    /// <param name="locale">
    /// The locale to answer in: <c>ru</c> or <c>en</c>. Defaults to <c>ru</c>. When the text has no
    /// translation in the requested locale, the default locale's text is returned and the response
    /// echoes the locale it actually came from.
    /// </param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <response code="200">The profile text (or null when not yet written).</response>
    /// <response code="400">The locale is not one the site publishes.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(OwnerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OwnerProfileResponse?>> GetProfile(
        [FromQuery] string? locale,
        CancellationToken cancellationToken)
    {
        if (!TryResolveLocale(locale, out var resolved))
        {
            return UnknownLocale(locale);
        }

        return await BuildProfileAsync(resolved, cancellationToken);
    }

    /// <summary>Returns the published projects with keyset pagination.</summary>
    /// <param name="locale">
    /// The locale to answer in: <c>ru</c> or <c>en</c>. Defaults to <c>ru</c>. A project whose text
    /// is missing in the requested locale falls back to the default locale's text.
    /// </param>
    /// <param name="cursor">Keyset cursor for pagination (format: <c>{year}:{id}</c>).</param>
    /// <param name="pageSize">Number of projects per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <response code="200">The paginated projects list.</response>
    /// <response code="400">Invalid locale or pagination parameters.</response>
    [HttpGet("projects")]
    [ProducesResponseType<PaginatedProjectsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedProjectsResponse>> GetProjects(
        [FromQuery] string? locale,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryResolveLocale(locale, out var resolved))
        {
            return UnknownLocale(locale);
        }

        pageSize = ClampPageSize(pageSize);

        return await BuildPaginatedProjectsAsync(resolved, cursor, pageSize, cancellationToken);
    }

    private async Task<OwnerProfileResponse?> BuildProfileAsync(
        Locale locale, CancellationToken cancellationToken)
    {
        var profile = await _content.GetProfileAsync(cancellationToken);
        var translation = profile?.TranslationFor(locale);

        if (translation is null)
        {
            return null;
        }

        return new OwnerProfileResponse(
            translation.Locale.Value,
            translation.Headline,
            translation.About);
    }

    private async Task<PaginatedProjectsResponse> BuildPaginatedProjectsAsync(
        Locale locale, string? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var projects = await _content.GetPublishedProjectsAsync(cursor, pageSize, cancellationToken);

        // Determine hasMore and nextCursor from raw objects before filtering by translation.
        var hasMore = projects.Count > pageSize;
        var displayProjects = hasMore ? projects.Take(pageSize).ToList() : [.. projects];

        // A project with no text in any locale is not shown: there would be nothing to render.
        var mapped = displayProjects
            .Select(project => Map(project, locale))
            .OfType<ProjectResponse>()
            .ToList();

        var nextCursor = hasMore && mapped.Count > 0
            ? EncodeCursor(displayProjects[mapped.Count - 1])
            : null;

        return new PaginatedProjectsResponse(mapped, nextCursor, hasMore);
    }

    private static ProjectResponse? Map(Project project, Locale locale)
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

    private static string? EncodeCursor(Project project)
    {
        return $"{project.Year}:{project.Id}";
    }

    private static bool TryResolveLocale(string? value, out Locale locale)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            locale = Locale.Default;
            return true;
        }

        if (Locale.TryParse(value, out var parsed) && parsed is not null)
        {
            locale = parsed;
            return true;
        }

        locale = Locale.Default;
        return false;
    }

    private ActionResult UnknownLocale(string? locale)
    {
        var supported = string.Join(", ", Locale.All.Select(candidate => candidate.Value));

        ModelState.AddModelError(
            nameof(locale),
            $"Unknown locale '{locale}'. Supported locales: {supported}.");

        return ValidationProblem(ModelState);
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
