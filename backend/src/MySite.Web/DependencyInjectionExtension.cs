using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySite.Infrastructure.Postgres;
using MySite.Web.HealthChecks;

namespace MySite.Web;

/// <summary>Composes the application: the web surface and the infrastructure behind it.</summary>
public static class DependencyInjectionExtension
{
    /// <summary>Adds everything the application needs to run.</summary>
    /// <param name="services">The collection the registrations are added to.</param>
    /// <param name="configuration">The configuration the layers below read from.</param>
    /// <returns>The same collection, so registrations can be chained.</returns>
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddWebDependencies().AddInfrastructure(configuration);

        return services;
    }

    /// <summary>Adds the transport: controllers, the OpenAPI document and the health probes.</summary>
    /// <param name="services">The collection the registrations are added to.</param>
    /// <returns>The same collection, so registrations can be chained.</returns>
    public static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddControllers();
        services.AddOpenApi();

        // One error shape for the whole API, in the RFC 7807 form.
        services.AddProblemDetails();

        // Liveness answers whether the process is up; readiness answers whether it can serve traffic.
        // The route filters on the tag, so a further readiness check is added by tagging it.
        services.AddHealthChecks()
            .AddCheck(HealthCheckNames.Self, () => HealthCheckResult.Healthy(), tags: [HealthCheckTags.Live])
            .AddCheck<PostgresHealthCheck>(HealthCheckNames.Postgres, tags: [HealthCheckTags.Ready]);

        return services;
    }
}
