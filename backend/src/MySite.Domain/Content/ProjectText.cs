namespace MySite.Domain.Content;

/// <summary>One project's text in one locale, as it arrives from the content file.</summary>
public sealed record ProjectText(Locale Locale, string Title, string Summary);
