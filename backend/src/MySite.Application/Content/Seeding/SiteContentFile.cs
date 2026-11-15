namespace MySite.Application.Content.Seeding;

/// <summary>The top-level shape of the content file, as deserialized from JSON.</summary>
internal sealed class SiteContentFile
{
    /// <summary>The owner profile section, or null when absent.</summary>
    public ProfileContent? Profile { get; init; }

    /// <summary>The list of projects, or null when absent.</summary>
    public IReadOnlyList<ProjectContent>? Projects { get; init; }
}

/// <summary>The profile section of the content file.</summary>
internal sealed class ProfileContent
{
    /// <summary>Translations keyed by locale code string.</summary>
    public Dictionary<string, ProfileTranslationContent>? Translations { get; init; }
}

/// <summary>A single locale's translation for the owner profile.</summary>
internal sealed class ProfileTranslationContent
{
    /// <summary>The translated headline.</summary>
    public string? Headline { get; init; }

    /// <summary>The translated about text.</summary>
    public string? About { get; init; }
}

/// <summary>A single project in the content file.</summary>
internal sealed class ProjectContent
{
    /// <summary>The project link, or null when absent.</summary>
    public string? Link { get; init; }

    /// <summary>The year this project belongs to.</summary>
    public int Year { get; init; }

    /// <summary>The sort order within the year.</summary>
    public int SortOrder { get; init; }

    /// <summary>Whether this project is published.</summary>
    public bool IsPublished { get; init; }

    /// <summary>The technology stack, or null when absent.</summary>
    public IReadOnlyList<string>? Stack { get; init; }

    /// <summary>Translations keyed by locale code string.</summary>
    public Dictionary<string, ProjectTranslationContent>? Translations { get; init; }
}

/// <summary>A single locale's translation for a project.</summary>
internal sealed class ProjectTranslationContent
{
    /// <summary>The translated title.</summary>
    public string? Title { get; init; }

    /// <summary>The translated summary.</summary>
    public string? Summary { get; init; }
}
