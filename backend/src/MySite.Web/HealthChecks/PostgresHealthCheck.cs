using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySite.Application.Health;

namespace MySite.Web.HealthChecks;

/// <summary>Reports whether the site can reach its database.</summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly IDatabaseProbe _database;

    /// <summary>Creates the check.</summary>
    /// <param name="database">Probes the database through the abstraction the application declares.</param>
    public PostgresHealthCheck(IDatabaseProbe database)
    {
        _database = database;
    }

    /// <summary>Asks the database whether it accepts a connection.</summary>
    /// <param name="context">The health check context, unused.</param>
    /// <param name="cancellationToken">Cancels the probe.</param>
    /// <returns>Healthy when the database answers, unhealthy otherwise.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database refused a connection.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("The database could not be reached.", exception);
        }
    }
}
