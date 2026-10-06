using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySite.Application.Content;
using MySite.Infrastructure.Postgres.Persistence;
using MySite.Infrastructure.Postgres.Persistence.Repositories;

namespace MySite.Infrastructure.Postgres;

/// <summary>Registers the adapters that reach the outside world.</summary>
public static class DependencyInjectionExtension
{
    /// <summary>The configuration key the connection string is read from.</summary>
    public const string ConnectionStringName = "DefaultConnection";

    /// <summary>Adds the database context and the repositories that run against it.</summary>
    /// <param name="services">The collection the registrations are added to.</param>
    /// <param name="configuration">The configuration the connection string is read from.</param>
    /// <returns>The same collection, so registrations can be chained.</returns>
    /// <exception cref="InvalidOperationException">The connection string is missing or empty.</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        // Stopping with a clear message beats starting and failing later on the first query.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"The connection string '{ConnectionStringName}' is missing or empty. Provide it as "
                + $"'ConnectionStrings:{ConnectionStringName}' in appsettings.Development.json or in an "
                + "environment variable.");
        }

        services.AddDbContext<MySiteDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IContentRepository, ContentRepository>();

        return services;
    }
}
