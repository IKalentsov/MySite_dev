namespace MySite.Domain.Content;

/// <summary>One language's version of a portfolio project's text.</summary>
public class ProjectTranslation
{
    // Entity Framework materialises through this constructor.
    private ProjectTranslation()
    {
    }

    private ProjectTranslation(Guid id, Guid projectId, Locale locale, string title, string summary)
    {
        Id = id;
        ProjectId = projectId;
        Locale = locale;
        Title = title;
        Summary = summary;
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Locale Locale { get; private set; } = Locale.Default;

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public static ProjectTranslation Create(Guid projectId, Locale locale, string title, string summary) =>
        new(Guid.NewGuid(), projectId, locale, title, summary);
}
