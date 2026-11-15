using System.Text.Json.Nodes;

namespace MySite.IntegrationTests;

/// <summary>Builds the content files the seed tests feed to the command.</summary>
internal static class SeedFiles
{
    internal const string FirstLink = "https://example.com/one";
    internal const string SecondLink = "https://example.com/two";

    internal static string TwoProjects() => Build(
    [
        new ProjectFile(FirstLink, "Первый проект", "First project", ["csharp", "postgres"]),
        new ProjectFile(SecondLink, "Второй проект", null, ["typescript"])
    ]);

    internal static string OneProject(bool includeEnglish) => Build(
    [
        new ProjectFile(
            FirstLink,
            "Первый проект",
            includeEnglish ? "First project" : null,
            includeEnglish ? ["csharp", "postgres"] : [])
    ]);

    internal static string OneProjectWithStack(params string[] stack) => Build(
    [
        new ProjectFile(FirstLink, "Первый проект", null, stack)
    ]);

    private static string Build(IReadOnlyList<ProjectFile> projects)
    {
        var projectNodes = new JsonArray();

        for (var index = 0; index < projects.Count; index++)
        {
            var project = projects[index];
            var translations = new JsonObject
            {
                ["ru"] = ProjectTranslation(project.Title)
            };

            if (project.EnglishTitle is not null)
            {
                translations["en"] = ProjectTranslation(project.EnglishTitle);
            }

            var stack = new JsonArray();
            foreach (var item in project.Stack)
            {
                stack.Add(item);
            }

            projectNodes.Add(new JsonObject
            {
                ["link"] = project.Link,
                ["year"] = 2026,
                ["sortOrder"] = index + 1,
                ["isPublished"] = true,
                ["stack"] = stack,
                ["translations"] = translations
            });
        }

        var file = new JsonObject
        {
            ["profile"] = new JsonObject
            {
                ["translations"] = new JsonObject
                {
                    ["ru"] = ProfileTranslation("ru headline", "ru about"),
                    ["en"] = ProfileTranslation("en headline", "en about")
                }
            },
            ["projects"] = projectNodes
        };

        return file.ToJsonString();
    }

    private static JsonObject ProjectTranslation(string title) => new()
    {
        ["title"] = title,
        ["summary"] = title + " summary"
    };

    private static JsonObject ProfileTranslation(string headline, string about) => new()
    {
        ["headline"] = headline,
        ["about"] = about
    };

    private sealed record ProjectFile(string Link, string Title, string? EnglishTitle, string[] Stack);
}
