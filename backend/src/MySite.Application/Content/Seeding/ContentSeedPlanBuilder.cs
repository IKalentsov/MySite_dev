using MySite.Domain.Content;

namespace MySite.Application.Content.Seeding;

/// <summary>Maps a validated content file into a seed plan for the infrastructure layer.</summary>
internal static class ContentSeedPlanBuilder
{
    /// <summary>Builds a seed plan from a validated content file.</summary>
    /// <param name="file">A content file that has already passed validation.</param>
    /// <returns>A seed plan ready for the infrastructure writer.</returns>
    internal static ContentSeedPlan Build(SiteContentFile file)
    {
        var profileTexts = MapProfileTexts(file.Profile);
        var projectSeeds = file.Projects?.Select(MapProjectSeed).ToList() ?? [];

        return new(new(profileTexts), projectSeeds);
    }

    private static IReadOnlyList<ProfileText> MapProfileTexts(ProfileContent? profile)
    {
        if (profile?.Translations is null)
        {
            return [];
        }

        return [.. profile.Translations.Select(pair =>
            new ProfileText(Locale.From(pair.Key), pair.Value.Headline!, pair.Value.About!))];
    }

    private static ProjectSeed MapProjectSeed(ProjectContent project)
    {
        var texts = MapProjectTexts(project.Translations);
        var stack = project.Stack ?? [];

        return new(
            project.Link!,
            project.Year,
            project.SortOrder,
            project.IsPublished,
            texts,
            stack);
    }

    private static IReadOnlyList<ProjectText> MapProjectTexts(
        Dictionary<string, ProjectTranslationContent>? translations)
    {
        if (translations is null)
        {
            return [];
        }

        return [.. translations.Select(pair =>
            new ProjectText(Locale.From(pair.Key), pair.Value.Title!, pair.Value.Summary!))];
    }
}
