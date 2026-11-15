using MySite.Domain.Content;
using Xunit;

namespace MySite.UnitTests.Domain.Content;

/// <summary>
/// The ADR-0005 rule: the requested locale first, the default locale second, nothing third.
/// Both entities that store per-locale text implement it, so both are checked.
/// </summary>
public sealed class TranslationForTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("ru", "ru title")]
    [InlineData("en", "en title")]
    public void ProjectTranslationForReturnsTheRequestedLocale(string requested, string expectedTitle)
    {
        var project = CreateProject();
        project.ReplaceTexts(
        [
            new ProjectText(Locale.Ru, "ru title", "ru summary"),
            new ProjectText(Locale.En, "en title", "en summary")
        ]);

        var translation = project.TranslationFor(Locale.From(requested));

        Assert.NotNull(translation);
        Assert.Equal(expectedTitle, translation.Title);
    }

    [Theory]
    [InlineData("en")]
    public void ProjectTranslationForFallsBackToTheDefaultLocale(string requested)
    {
        var project = CreateProject();
        project.ReplaceTexts([new ProjectText(Locale.Ru, "ru title", "ru summary")]);

        var translation = project.TranslationFor(Locale.From(requested));

        Assert.NotNull(translation);
        Assert.Equal("ru title", translation.Title);
    }

    [Theory]
    [InlineData("ru")]
    public void ProjectTranslationForReturnsNothingWhenNeitherLocaleHasText(string requested)
    {
        var project = CreateProject();
        project.ReplaceTexts([new ProjectText(Locale.En, "en title", "en summary")]);

        Assert.Null(project.TranslationFor(Locale.From(requested)));
    }

    [Theory]
    [InlineData("ru", "ru headline")]
    [InlineData("en", "en headline")]
    public void ProfileTranslationForReturnsTheRequestedLocale(string requested, string expectedHeadline)
    {
        var profile = OwnerProfile.Create(Guid.NewGuid(), UtcNow, UtcNow);
        profile.ReplaceTexts(
        [
            new ProfileText(Locale.Ru, "ru headline", "ru about"),
            new ProfileText(Locale.En, "en headline", "en about")
        ]);

        var translation = profile.TranslationFor(Locale.From(requested));

        Assert.NotNull(translation);
        Assert.Equal(expectedHeadline, translation.Headline);
    }

    [Theory]
    [InlineData("en")]
    public void ProfileTranslationForFallsBackToTheDefaultLocale(string requested)
    {
        var profile = OwnerProfile.Create(Guid.NewGuid(), UtcNow, UtcNow);
        profile.ReplaceTexts([new ProfileText(Locale.Ru, "ru headline", "ru about")]);

        var translation = profile.TranslationFor(Locale.From(requested));

        Assert.NotNull(translation);
        Assert.Equal("ru headline", translation.Headline);
    }

    private static Project CreateProject() =>
        Project.Create(Guid.NewGuid(), "https://example.com/p", 2026, 1, true, UtcNow, UtcNow);
}
