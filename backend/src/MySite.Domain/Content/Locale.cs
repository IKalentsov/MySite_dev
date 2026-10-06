namespace MySite.Domain.Content;

/// <summary>
/// A content locale. Only the locales the site actually publishes exist as values, so an unknown
/// locale cannot travel through the application.
/// </summary>
public sealed record Locale
{
    private Locale(string value) => Value = value;

    public static Locale Ru { get; } = new("ru");

    public static Locale En { get; } = new("en");

    /// <summary>The locale content falls back to when a translation is missing.</summary>
    public static Locale Default => Ru;

    public static IReadOnlyList<Locale> All { get; } = [Ru, En];

    public string Value { get; }

    public static bool TryParse(string? value, out Locale? locale)
    {
        locale = All.FirstOrDefault(candidate =>
            string.Equals(candidate.Value, value, StringComparison.OrdinalIgnoreCase));

        return locale is not null;
    }

    public static Locale From(string value) =>
        TryParse(value, out var locale)
            ? locale!
            : throw new ArgumentException($"Unknown locale '{value}'.", nameof(value));

    public override string ToString() => Value;
}
