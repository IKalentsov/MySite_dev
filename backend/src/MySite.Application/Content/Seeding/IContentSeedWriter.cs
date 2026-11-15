namespace MySite.Application.Content.Seeding;

/// <summary>Writes a validated seed plan to storage, making the stored content converge to it.</summary>
public interface IContentSeedWriter
{
    /// <summary>Applies the plan inside one transaction and reports what changed.</summary>
    /// <param name="plan">The validated seed plan to apply.</param>
    /// <param name="cancellationToken">Cancels the write when the caller disconnects.</param>
    /// <returns>Describes what the operation created, updated, or left unchanged.</returns>
    Task<ContentSeedOutcome> SeedAsync(ContentSeedPlan plan, CancellationToken cancellationToken);
}
