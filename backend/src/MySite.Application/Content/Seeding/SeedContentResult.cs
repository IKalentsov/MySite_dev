namespace MySite.Application.Content.Seeding;

/// <summary>The result of a seed operation, carrying status and details.</summary>
public sealed class SeedContentResult
{
    private SeedContentResult(
        SeedContentStatus status,
        ContentSeedOutcome? outcome,
        IReadOnlyList<string> translationGaps,
        IReadOnlyList<string> errors)
    {
        Status = status;
        Outcome = outcome;
        TranslationGaps = translationGaps;
        Errors = errors;
    }

    /// <summary>The overall status of the seed operation.</summary>
    public SeedContentStatus Status { get; }

    /// <summary>Details about what changed, present only when <see cref="Status"/> is <see cref="SeedContentStatus.Seeded"/>.</summary>
    public ContentSeedOutcome? Outcome { get; }

    /// <summary>Projects the file leaves in a state where the default locale text is shown instead.</summary>
    public IReadOnlyList<string> TranslationGaps { get; }

    /// <summary>Error messages, present only when <see cref="Status"/> is <see cref="SeedContentStatus.Invalid"/>.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Creates a successful seed result.</summary>
    /// <param name="outcome">Details about what changed in storage.</param>
    /// <param name="translationGaps">Projects missing the English translation.</param>
    /// <returns>A seeded result with no errors.</returns>
    public static SeedContentResult Seeded(
        ContentSeedOutcome outcome,
        IReadOnlyList<string> translationGaps) =>
        new(SeedContentStatus.Seeded, outcome, translationGaps, []);

    /// <summary>Creates an invalid seed result from validation errors.</summary>
    /// <param name="errors">The errors that prevented the seed.</param>
    /// <returns>An invalid result with no outcome or gaps.</returns>
    public static SeedContentResult Invalid(IReadOnlyList<string> errors) =>
        new(SeedContentStatus.Invalid, null, [], errors);
}
