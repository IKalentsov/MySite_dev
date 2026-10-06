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
    private readonly IContentRepository _content;

    /// <summary>Creates the controller.</summary>
    /// <param name="content">Reads the site's published content.</param>
    public ContentController(IContentRepository content)
    {
        _content = content;
    }

    /// <summary>Returns the owner's "about me" text.</summary>
    /// <param name="locale">
    /// The locale to answer in: <c>ru</c> or <c>en</c>. Defaults to <c>ru</c>. When the text has no
    /// translation in the requested locale, the default locale's text is returned and the response
    /// echoes the locale it actually came from.
    /// </param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <response code="200">The profile text.</response>
    /// <response code="400">The locale is not one the site publishes.</response>
    /// <response code="404">No profile has been written yet.</response>
    [HttpGet("profile")]
    [ProducesResponseType<OwnerProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OwnerProfileResponse>> GetProfile(
        [FromQuery] string? locale,
        CancellationToken cancellationToken)
    {
        if (!TryResolveLocale(locale, out var resolved))
        {
            return UnknownLocale(locale);
        }

        var profile = await _content.GetProfileAsync(cancellationToken);
        var translation = profile?.TranslationFor(resolved);

        if (translation is null)
        {
            return NotFound();
        }

        return Ok(new OwnerProfileResponse(
            translation.Locale.Value,
            translation.Headline,
            translation.About));
    }

    /// <summary>Returns the published projects.</summary>
    /// <param name="locale">
    /// The locale to answer in: <c>ru</c> or <c>en</c>. Defaults to <c>ru</c>. A project whose text
    /// is missing in the requested locale falls back to the default locale's text.
    /// </param>
    /// <param name="cancellationToken">Cancels the read when the caller disconnects.</param>
    /// <response code="200">The projects, empty when none are published.</response>
    /// <response code="400">The locale is not one the site publishes.</response>
    [HttpGet("projects")]
    [ProducesResponseType<ProjectListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectListResponse>> GetProjects(
        [FromQuery] string? locale,
        CancellationToken cancellationToken)
    {
        if (!TryResolveLocale(locale, out var resolved))
        {
            return UnknownLocale(locale);
        }

        var projects = await _content.GetPublishedProjectsAsync(cancellationToken);

        // A project with no text in any locale is not shown: there would be nothing to render.
        var published = projects
            .Select(project => Map(project, resolved))
            .OfType<ProjectResponse>()
            .ToList();

        return Ok(new ProjectListResponse(published));
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
}
