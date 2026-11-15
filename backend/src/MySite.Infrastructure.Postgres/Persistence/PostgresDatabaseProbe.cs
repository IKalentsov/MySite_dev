using Microsoft.EntityFrameworkCore;
using MySite.Application.Health;

namespace MySite.Infrastructure.Postgres.Persistence;

/// <summary>Asks PostgreSQL whether it accepts a connection.</summary>
public sealed class PostgresDatabaseProbe : IDatabaseProbe
{
    private readonly MySiteDbContext _context;

    /// <summary>Creates the probe.</summary>
    /// <param name="context">The context whose connection is probed.</param>
    public PostgresDatabaseProbe(MySiteDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken) =>
        _context.Database.CanConnectAsync(cancellationToken);
}
