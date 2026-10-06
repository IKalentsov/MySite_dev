namespace MySite.Domain.Content;

/// <summary>
/// The site's "about me" content. There is exactly one, and its text exists per locale.
/// </summary>
public class OwnerProfile
{
    private readonly List<OwnerProfileTranslation> _translations = [];

    // Entity Framework materialises through this constructor.
    private OwnerProfile()
    {
    }

    private OwnerProfile(Guid id, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OwnerProfileTranslation> Translations => _translations;

    public static OwnerProfile Create(Guid id, DateTime createdAtUtc, DateTime updatedAtUtc) =>
        new(id, createdAtUtc, updatedAtUtc);

    /// <summary>
    /// The text in the requested locale, or the default locale's text when that translation is
    /// missing, or nothing when neither exists. The rule lives here and not in SQL.
    /// </summary>
    public OwnerProfileTranslation? TranslationFor(Locale locale) =>
        _translations.Find(translation => translation.Locale == locale)
        ?? _translations.Find(translation => translation.Locale == Locale.Default);
}
