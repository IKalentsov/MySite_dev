namespace MySite.Domain.Content;

/// <summary>A piece of work the owner built and shows on the site.</summary>
public class Project
{
    private readonly List<ProjectTranslation> _translations = [];
    private readonly List<ProjectStackItem> _stack = [];

    // Entity Framework materialises through this constructor.
    private Project()
    {
    }

    private Project(
        Guid id,
        string link,
        int year,
        int sortOrder,
        bool isPublished,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        Link = link;
        Year = year;
        SortOrder = sortOrder;
        IsPublished = isPublished;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Link { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ProjectTranslation> Translations => _translations;

    public IReadOnlyCollection<ProjectStackItem> Stack => _stack;

    public static Project Create(
        Guid id,
        string link,
        int year,
        int sortOrder,
        bool isPublished,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) =>
        new(id, link, year, sortOrder, isPublished, createdAtUtc, updatedAtUtc);

    /// <summary>
    /// The text in the requested locale, or the default locale's text when that translation is
    /// missing, or nothing when neither exists.
    /// </summary>
    public ProjectTranslation? TranslationFor(Locale locale) =>
        _translations.Find(translation => translation.Locale == locale)
        ?? _translations.Find(translation => translation.Locale == Locale.Default);

    /// <summary>Replaces the language-independent fields.</summary>
    /// <param name="year">The year this project belongs to.</param>
    /// <param name="sortOrder">The order in which projects are displayed within a year.</param>
    /// <param name="isPublished">Whether this project is visible on the public page.</param>
    /// <param name="updatedAtUtc">The moment of this update, recorded as UTC.</param>
    public void Update(int year, int sortOrder, bool isPublished, DateTime updatedAtUtc)
    {
        Year = year;
        SortOrder = sortOrder;
        IsPublished = isPublished;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Makes the translation rows exactly the given locales: existing rows for those locales are
    /// updated in place, missing locales are added, and locales absent from <paramref name="texts"/>
    /// are removed.
    /// </summary>
    /// <param name="texts">The texts to converge to, keyed by locale.</param>
    public void ReplaceTexts(IReadOnlyCollection<ProjectText> texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var textsByLocale = texts.ToDictionary(t => t.Locale);

        foreach (var translation in _translations.ToList())
        {
            if (textsByLocale.TryGetValue(translation.Locale, out var newText))
            {
                translation.Update(newText.Title, newText.Summary);
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
                _translations.Add(ProjectTranslation.Create(Id, locale, text.Title, text.Summary));
            }
        }
    }

    /// <summary>
    /// Makes the stack exactly the given names in order: rows are updated in place by position,
    /// missing positions are added, extra positions are removed.
    /// </summary>
    /// <param name="names">The stack item names in display order.</param>
    public void ReplaceStack(IReadOnlyCollection<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        var namesList = names.ToList();

        _stack.RemoveAll(item => item.Position >= namesList.Count);

        for (var position = 0; position < namesList.Count; position++)
        {
            var existing = _stack.Find(item => item.Position == position);

            if (existing is null)
            {
                _stack.Add(ProjectStackItem.Create(Id, namesList[position], position));
            }
            else
            {
                existing.Update(namesList[position]);
            }
        }
    }
}
