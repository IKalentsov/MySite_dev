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
}
