namespace MySite.Application.Health;

/// <summary>
/// Answers whether the site can reach its database. The readiness probe goes through this
/// abstraction, so the transport never depends on a persistence type and the adapter that knows
/// how to reach the database stays in the infrastructure.
/// </summary>
public interface IDatabaseProbe
{
    /// <summary>Asks the database whether it accepts a connection.</summary>
    /// <param name="cancellationToken">Cancels the probe.</param>
    /// <returns>Whether the database answered.</returns>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}
