using MySite.Domain.Content;
using Xunit;

namespace MySite.UnitTests.Domain.Content;

public sealed class OwnerProfileTests
{
    private static readonly DateTime Earlier = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Later = new(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ReplaceTextsUpdatesInPlace()
    {
        var profile = OwnerProfile.Create(Guid.NewGuid(), Earlier, Earlier);

        profile.ReplaceTexts(new[]
        {
            new ProfileText(Locale.Ru, "ru headline", "ru about"),
            new ProfileText(Locale.En, "en headline", "en about")
        });

        Assert.Equal(2, profile.Translations.Count);

        var ruTranslation = profile.Translations.First(t => t.Locale.Equals(Locale.Ru));
        var expectedId = ruTranslation.Id;

        profile.ReplaceTexts(new[]
        {
            new ProfileText(Locale.Ru, "new headline", "ru about")
        });

        Assert.Single(profile.Translations);
        var remaining = profile.Translations.First();
        Assert.True(remaining.Locale.Equals(Locale.Ru));
        Assert.Equal("new headline", remaining.Headline);
        Assert.Equal(expectedId, remaining.Id);
    }

    [Fact]
    public void MarkUpdatedChangesTimestamp()
    {
        var profile = OwnerProfile.Create(Guid.NewGuid(), Earlier, Earlier);

        profile.MarkUpdated(Later);

        Assert.Equal(Later, profile.UpdatedAtUtc);
        Assert.Equal(Earlier, profile.CreatedAtUtc);
    }
}
