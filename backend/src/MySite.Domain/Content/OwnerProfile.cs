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

    /// <summary>Replaces the profile's text for the given locales, in place where possible.</summary>
    /// <param name="texts">The texts to converge to, keyed by locale.</param>
    public void ReplaceTexts(IReadOnlyCollection<ProfileText> texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var textsByLocale = texts.ToDictionary(t => t.Locale);

        foreach (var translation in _translations.ToList())
        {
            if (textsByLocale.TryGetValue(translation.Locale, out var newText))
            {
                translation.Update(newText.Headline, newText.About);
            }
            else
            {
                _translations.Remove(translation);
            }
        }

        foreach (var (locale, text) in textsByLocale)
        {
            if (_translations.All(t => t.Locale != locale))
            {
                _translations.Add(OwnerProfileTranslation.Create(Id, locale, text.Headline, text.About));
            }
        }
    }

    /// <summary>Records that the profile changed.</summary>
    /// <param name="updatedAtUtc">The moment of this update, recorded as UTC.</param>
    public void MarkUpdated(DateTime updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
    }
}
