using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using MySite.Application.Content.Seeding;
using MySite.Domain.Content;
using NSubstitute;
using Xunit;

namespace MySite.UnitTests.Content.Seeding;

public sealed class SeedContentServiceTests
{
    private const string ValidContent = """
    {
      "profile": {
        "translations": {
          "ru": { "headline": "Заглушка: заголовок", "about": "Заглушка: текст" },
          "en": { "headline": "Placeholder: headline", "about": "Placeholder: about" }
        }
      },
      "projects": [
        {
          "link": "https://example.com/one",
          "year": 2026,
          "sortOrder": 1,
          "isPublished": true,
          "stack": [".NET", "PostgreSQL"],
          "translations": {
            "ru": { "title": "Заглушка: один", "summary": "Заглушка: описание" },
            "en": { "title": "Placeholder: one", "summary": "Placeholder: summary" }
          }
        }
      ]
    }
    """;

    [Fact]
    public async Task SeedsValidContent()
    {
        var writer = Substitute.For<IContentSeedWriter>();
        ContentSeedPlan? captured = null;
        writer.SeedAsync(
                Arg.Do<ContentSeedPlan>(plan => captured = plan),
                Arg.Any<CancellationToken>())
            .Returns(new ContentSeedOutcome(true, false, 1, 0, 0, Array.Empty<string>()));

        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(ValidContent, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Seeded, result.Status);
        Assert.Empty(result.Errors);
        Assert.NotNull(captured);

        var expectedProfileLocales = new[] { "ru", "en" };
        Assert.Equal(expectedProfileLocales.Length, captured.Profile.Texts.Count);
        var actualProfileLocales = captured.Profile.Texts.Select(t => t.Locale.Value).ToList();
        Assert.Equal(expectedProfileLocales, actualProfileLocales, StringComparer.Ordinal);

        Assert.Single(captured.Projects);
        var project = captured.Projects[0];
        Assert.Equal("https://example.com/one", project.Link);
        Assert.Equal(2026, project.Year);
        Assert.Equal(1, project.SortOrder);
        Assert.True(project.IsPublished);

        var expectedStack = new[] { ".NET", "PostgreSQL" };
        Assert.Equal(expectedStack.Length, project.Stack.Count);
        Assert.Equal(expectedStack, project.Stack, StringComparer.Ordinal);

        Assert.Equal(2, project.Texts.Count);
        var textLocales = project.Texts.Select(t => t.Locale.Value).OrderBy(l => l, StringComparer.Ordinal).ToList();
        var expectedTextLocales = new[] { "en", "ru" };
        Assert.Equal(expectedTextLocales, textLocales, StringComparer.Ordinal);

        var ruText = project.Texts.First(t => t.Locale.Equals(Locale.Ru));
        Assert.Equal("Заглушка: один", ruText.Title);
    }

    [Fact]
    public async Task ReportsMissingEnglishAsGap()
    {
        // Remove the "en" block from the project's translations.
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        var projects = doc["projects"]!.AsArray();
        var projectTranslations = (JsonObject)projects[0]!["translations"]!;
        projectTranslations.Remove("en");

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        ContentSeedPlan? captured = null;
        writer.SeedAsync(
                Arg.Do<ContentSeedPlan>(plan => captured = plan),
                Arg.Any<CancellationToken>())
            .Returns(new ContentSeedOutcome(true, false, 1, 0, 0, Array.Empty<string>()));

        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Seeded, result.Status);
        Assert.Contains(result.TranslationGaps, g => g.Contains("https://example.com/one", StringComparison.Ordinal));

        Assert.NotNull(captured);
        Assert.Single(captured.Projects);
        var project = captured.Projects[0];
        Assert.Single(project.Texts);
        Assert.Single(project.Texts, t => t.Locale.Equals(Locale.Ru));

        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RejectsUnknownLocale()
    {
        // Add "de" to the project's translations.
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        var projects = doc["projects"]!.AsArray();
        var projectTranslations = (JsonObject)projects[0]!["translations"]!;
        projectTranslations["de"] = new JsonObject
        {
            ["title"] = "...",
            ["summary"] = "..."
        };

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("de", StringComparison.Ordinal));
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsDuplicateLink()
    {
        // Duplicate the project object so two entries share the link.
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        var projects = doc["projects"]!.AsArray();
        var firstProject = projects[0]!;
        var clone = JsonNode.Parse(firstProject.ToJsonString())!;
        projects.Add(clone);

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsMissingDefaultLocale()
    {
        // Replace the project's translations with only "en" (missing default locale "ru").
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        var projects = doc["projects"]!.AsArray();
        var projectTranslations = (JsonObject)projects[0]!["translations"]!;
        projectTranslations.Remove("ru");

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        var allErrors = string.Join(" ", result.Errors);
        Assert.Contains("default locale", allErrors, StringComparison.Ordinal);
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsEmptyRequiredField()
    {
        // Set the project's ru title to whitespace.
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        var projects = doc["projects"]!.AsArray();
        var projectTranslations = (JsonObject)projects[0]!["translations"]!;
        projectTranslations["ru"]!["title"] = "   ";

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("title", StringComparison.Ordinal));
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsMissingProfile()
    {
        // Remove the whole "profile" section.
        var doc = (JsonObject)JsonNode.Parse(ValidContent)!;
        doc.Remove("profile");

        var json = doc.ToJsonString();

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("profile", StringComparison.Ordinal));
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsInvalidJson()
    {
        var json = "{ not json";

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.Single(result.Errors);
        Assert.Contains("not valid JSON", result.Errors[0], StringComparison.OrdinalIgnoreCase);
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectsDuplicateLocaleKey()
    {
        // The project's translations object contains "ru" twice with different titles.
        var json = """
        {
          "profile": {
            "translations": {
              "ru": { "headline": "x", "about": "y" },
              "en": { "headline": "a", "about": "b" }
            }
          },
          "projects": [
            {
              "link": "https://example.com/one",
              "year": 2026,
              "sortOrder": 1,
              "isPublished": true,
              "stack": [],
              "translations": {
                "ru": { "title": "Заглушка: один", "summary": "Заглушка: описание" },
                "ru": { "title": "Дубликат: заголовок", "summary": "Дубликат: текст" },
                "en": { "title": "Placeholder: one", "summary": "Placeholder: summary" }
              }
            }
          ]
        }
        """;

        var writer = Substitute.For<IContentSeedWriter>();
        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Invalid, result.Status);
        Assert.NotEmpty(result.Errors);
        await writer.DidNotReceiveWithAnyArgs().SeedAsync(Arg.Any<ContentSeedPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShippedContentFileParsesAndValidates()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "content", "site-content.json");
        var json = await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken);

        var writer = Substitute.For<IContentSeedWriter>();
        ContentSeedPlan? captured = null;
        writer.SeedAsync(
                Arg.Do<ContentSeedPlan>(plan => captured = plan),
                Arg.Any<CancellationToken>())
            .Returns(new ContentSeedOutcome(true, false, 1, 0, 0, Array.Empty<string>()));

        var service = new SeedContentService(writer);

        var result = await service.SeedAsync(json, TestContext.Current.CancellationToken);

        Assert.Equal(SeedContentStatus.Seeded, result.Status);
        Assert.Empty(result.Errors);

        await writer.Received(1).SeedAsync(
            Arg.Any<ContentSeedPlan>(),
            Arg.Any<CancellationToken>());

        Assert.NotNull(captured);
        var projects = captured.Projects.ToList();
        Assert.True(projects.Count >= 3 && projects.Count <= 5, $"Expected 3-5 projects but got {projects.Count}");

        var singleTextProjects = projects.Where(p => p.Texts.Count == 1).ToList();
        Assert.Single(singleTextProjects);
        Assert.True(singleTextProjects[0].Texts[0].Locale.Equals(Locale.Ru));

        var unpublished = projects.Where(p => !p.IsPublished).ToList();
        Assert.Single(unpublished);

        Assert.Equal(2, captured.Profile.Texts.Count);

        Assert.Contains(projects, p => p.Stack.Count > 0);
    }
}
