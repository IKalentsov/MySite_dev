using System.Text.Json;

namespace MySite.Application.Content.Seeding;

/// <summary>Orchestrates the seed pipeline: parse, validate, build plan, write.</summary>
public sealed class SeedContentService : ISeedContentService
{
    private readonly IContentSeedWriter _writer;

    /// <summary>Creates the service with the specified writer.</summary>
    /// <param name="writer">Writes the validated plan to storage.</param>
    public SeedContentService(IContentSeedWriter writer)
    {
        _writer = writer;
    }

    /// <inheritdoc />
    public async Task<SeedContentResult> SeedAsync(
        string json, CancellationToken cancellationToken)
    {
        SiteContentFile file;

        try
        {
            file = ContentSeedParser.Parse(json);
        }
        catch (JsonException exception)
        {
            return SeedContentResult.Invalid(
                [$"The content file is not valid JSON: {exception.Message}"]);
        }

        var validation = ContentSeedValidator.Validate(file);

        if (!validation.IsValid)
        {
            return SeedContentResult.Invalid(validation.Errors);
        }

        var plan = ContentSeedPlanBuilder.Build(file);
        var outcome = await _writer.SeedAsync(plan, cancellationToken);

        return SeedContentResult.Seeded(outcome, validation.TranslationGaps);
    }
}
