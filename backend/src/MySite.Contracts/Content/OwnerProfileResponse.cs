namespace MySite.Contracts.Content;

/// <summary>
/// The owner's "about me" text, already resolved to one locale. The locale is echoed back so a
/// caller can tell which text it received when the requested translation was missing.
/// </summary>
/// <param name="Locale">The locale the text was resolved to.</param>
/// <param name="Headline">The one-line description of the owner.</param>
/// <param name="About">The body of the "about me" text.</param>
public sealed record OwnerProfileResponse(string Locale, string Headline, string About);
