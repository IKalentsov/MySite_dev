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
    private readonly IContentService _content;

    /// <summary>Creates the controller.</summary>
    /// <param name="content">Serves the content the public page shows.</param>
    public ContentController(IContentService content)
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

        if (!TryResolveCursor(cursor, out var parsed))
        {
            return InvalidCursor(cursor);
        }

        return Ok(await _content.GetHomeAsync(resolved, parsed, pageSize, cancellationToken));
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

        return Ok(await _content.GetProfileAsync(resolved, cancellationToken));
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

        if (!TryResolveCursor(cursor, out var parsed))
        {
            return InvalidCursor(cursor);
        }

        return Ok(await _content.GetProjectsAsync(resolved, parsed, pageSize, cancellationToken));
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

    private static bool TryResolveCursor(string? value, out ProjectCursor? cursor)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            cursor = null;
            return true;
        }

        return ProjectCursor.TryParse(value, out cursor);
    }

    private ActionResult UnknownLocale(string? locale)
    {
        var supported = string.Join(", ", Locale.All.Select(candidate => candidate.Value));

        ModelState.AddModelError(
            nameof(locale),
            $"Unknown locale '{locale}'. Supported locales: {supported}.");

        return ValidationProblem(ModelState);
    }

    private ActionResult InvalidCursor(string? cursor)
    {
        ModelState.AddModelError(
            nameof(cursor),
            $"Invalid cursor '{cursor}'. Expected format: {{year}}:{{id}}.");

        return ValidationProblem(ModelState);
    }
}
