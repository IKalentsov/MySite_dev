using System.Globalization;
using MySite.Domain.Content;

namespace MySite.Application.Content.Seeding;

/// <summary>Validation result carrying errors and translation gaps.</summary>
internal sealed record SeedValidation(IReadOnlyList<string> Errors, IReadOnlyList<string> TranslationGaps)
{
    /// <summary>Whether there are no validation errors.</summary>
    internal bool IsValid => Errors.Count == 0;
}

/// <summary>Pure validation of a parsed content file. No I/O, no clock, no database.</summary>
internal static class ContentSeedValidator
{
    private static readonly string[] SupportedLocales = Locale.All.Select(l => l.Value).ToArray();

    /// <summary>Validates the parsed content file against all rules from ADR-0007.</summary>
    /// <param name="file">The parsed content file structure.</param>
    /// <returns>The validation result with errors and translation gaps.</returns>
    internal static SeedValidation Validate(SiteContentFile file)
    {
        var errors = new List<string>();
        var gaps = new List<string>();

        ValidateProfile(file, errors, gaps);
        ValidateProjects(file, errors, gaps);

        return new(errors, gaps);
    }

    private static void ValidateProfile(SiteContentFile file, List<string> errors, List<string> gaps)
    {
        if (file.Profile is null)
        {
            errors.Add("The content file has no profile section.");
            return;
        }

        var translations = file.Profile.Translations;

        if (translations is null || translations.Count == 0)
        {
            errors.Add("The profile has no translations.");
            return;
        }

        foreach (var key in translations.Keys)
        {
            ValidateLocaleKey(key, "profile", null, errors);
        }

        if (!HasLocale(translations, Locale.Default))
        {
            errors.Add(
                $"The profile is missing a translation for the default locale '{Locale.Default.Value}'.");
        }

        foreach (var kvp in translations)
        {
            if (Locale.TryParse(kvp.Key, out var locale) && locale is not null)
            {
                ValidateProfileText(locale.Value, kvp.Value.Headline, kvp.Value.About, errors);
            }
        }

        if (!HasLocale(translations, Locale.En))
        {
            gaps.Add("profile (" + Locale.En.Value + ")");
        }
    }

    private static void ValidateProjects(SiteContentFile file, List<string> errors, List<string> gaps)
    {
        var projects = file.Projects;
        if (projects is null || projects.Count == 0)
        {
            return;
        }

        var seenLinks = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < projects.Count; i++)
        {
            var project = projects[i];
            var index = i + 1;

            if (string.IsNullOrWhiteSpace(project.Link))
            {
                errors.Add(
                    $"Project {index.ToString(CultureInfo.InvariantCulture)} has an empty link.");
                continue;
            }

            var link = project.Link;

            if (!seenLinks.Add(link))
            {
                errors.Add($"Duplicate project link '{link}'.");
                continue;
            }

            ValidateProject(link, project, errors, gaps);
        }
    }

    private static void ValidateProject(
        string link, ProjectContent project, List<string> errors, List<string> gaps)
    {
        var translations = project.Translations;

        if (translations is null || translations.Count == 0)
        {
            errors.Add($"The project at '{link}' has no translations.");
            return;
        }

        foreach (var key in translations.Keys)
        {
            ValidateLocaleKey(key, "project", link, errors);
        }

        if (!HasLocale(translations, Locale.Default))
        {
            errors.Add(
                $"The project at '{link}' is missing a translation for the default locale '{Locale.Default.Value}'.");
        }

        foreach (var kvp in translations)
        {
            if (Locale.TryParse(kvp.Key, out var locale) && locale is not null)
            {
                ValidateProjectText(link, locale.Value, kvp.Value.Title, kvp.Value.Summary, errors);
            }
        }

        if (!HasLocale(translations, Locale.En))
        {
            gaps.Add(link + " (" + Locale.En.Value + ")");
        }

        var stack = project.Stack;
        if (stack != null)
        {
            for (var j = 0; j < stack.Count; j++)
            {
                if (string.IsNullOrWhiteSpace(stack[j]))
                {
                    errors.Add(
                        $"The project at '{link}' has an empty stack item at position {j.ToString(CultureInfo.InvariantCulture)}.");
                }
            }
        }
    }

    private static void ValidateLocaleKey(
        string key, string entityName, string? link, List<string> errors)
    {
        if (!Locale.TryParse(key, out _))
        {
            var supported = string.Join(", ", SupportedLocales);
            var identifier = link is not null ? $"The project at '{link}'" : $"The {entityName}";
            errors.Add($"{identifier} contains an unknown locale '{key}'. Supported locales are: {supported}.");
        }
    }

    private static void ValidateProfileText(
        string locale, string? headline, string? about, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(headline))
        {
            errors.Add($"The profile's {locale} headline is empty.");
        }

        if (string.IsNullOrWhiteSpace(about))
        {
            errors.Add($"The profile's {locale} about text is empty.");
        }
    }

    private static void ValidateProjectText(
        string link, string locale, string? title, string? summary, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add($"The project at '{link}' has an empty title for locale '{locale}'.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            errors.Add($"The project at '{link}' has an empty summary for locale '{locale}'.");
        }
    }

    private static bool HasLocale<T>(Dictionary<string, T> translations, Locale target)
    {
        return translations.Keys.Any(key =>
            Locale.TryParse(key, out var locale) && locale == target);
    }
}
