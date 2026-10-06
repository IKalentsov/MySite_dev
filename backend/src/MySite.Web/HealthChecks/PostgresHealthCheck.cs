using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySite.Infrastructure.Postgres.Persistence;

namespace MySite.Web.HealthChecks;

/// <summary>Reports whether the site can reach its database.</summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly MySiteDbContext _context;

    /// <summary>Creates the check.</summary>
    /// <param name="context">The context whose connection is probed.</param>
    public PostgresHealthCheck(MySiteDbContext context)
    {
        _context = context;
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
            return await _context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database refused a connection.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("The database could not be reached.", exception);
        }
    }
}
