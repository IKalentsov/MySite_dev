namespace MySite.Domain.Content;

/// <summary>The owner profile's text in one locale, as it arrives from the content file.</summary>
public sealed record ProfileText(Locale Locale, string Headline, string About);
