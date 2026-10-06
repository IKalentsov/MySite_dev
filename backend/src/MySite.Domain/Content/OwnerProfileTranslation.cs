namespace MySite.Domain.Content;

/// <summary>One language's version of the owner profile's text.</summary>
public class OwnerProfileTranslation
{
    // Entity Framework materialises through this constructor.
    private OwnerProfileTranslation()
    {
    }

    private OwnerProfileTranslation(
        Guid id,
        Guid ownerProfileId,
        Locale locale,
        string headline,
        string about)
    {
        Id = id;
        OwnerProfileId = ownerProfileId;
        Locale = locale;
        Headline = headline;
        About = about;
    }

    public Guid Id { get; private set; }

    public Guid OwnerProfileId { get; private set; }

    public Locale Locale { get; private set; } = Locale.Default;

    public string Headline { get; private set; } = string.Empty;

    public string About { get; private set; } = string.Empty;

    public static OwnerProfileTranslation Create(
        Guid ownerProfileId,
        Locale locale,
        string headline,
        string about) =>
        new(Guid.NewGuid(), ownerProfileId, locale, headline, about);
}
