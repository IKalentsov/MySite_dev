namespace MySite.Application.Content.Seeding;

/// <summary>Reads the content file's JSON, validates it and writes it to storage.</summary>
public interface ISeedContentService
{
    /// <summary>Reads the content file's JSON, validates it and writes it to storage.</summary>
    /// <param name="json">The JSON content from the file.</param>
    /// <param name="cancellationToken">Cancels the operation when the caller disconnects.</param>
    /// <returns>The outcome, or the validation errors that stopped it.</returns>
    Task<SeedContentResult> SeedAsync(string json, CancellationToken cancellationToken);
}
