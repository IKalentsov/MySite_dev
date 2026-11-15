using Microsoft.Extensions.DependencyInjection;
using MySite.Application.Content;
using MySite.Application.Content.Seeding;

namespace MySite.Application;

/// <summary>Composes the application layer: the use cases it offers.</summary>
public static class DependencyInjectionExtension
{
    /// <summary>Adds the use cases.</summary>
    /// <param name="services">The collection the registrations are added to.</param>
    /// <returns>The same collection, so registrations can be chained.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<ISeedContentService, SeedContentService>();

        return services;
    }
}
