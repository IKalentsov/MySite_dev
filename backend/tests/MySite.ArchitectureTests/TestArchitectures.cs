using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.Domain;

namespace MySite.ArchitectureTests;

internal static class TestArchitectures
{
    public static Architecture Production { get; } = new ArchLoader()
        .LoadAssemblies(
            typeof(MySite.Domain.Content.Locale).Assembly,
            typeof(MySite.Application.Content.IContentService).Assembly,
            typeof(MySite.Contracts.Content.HomeResponse).Assembly,
            typeof(MySite.Infrastructure.Postgres.Persistence.MySiteDbContext).Assembly,
            typeof(MySite.Web.DependencyInjectionExtension).Assembly)
        .Build();

    public static Architecture WithNegativeFixture { get; } = new ArchLoader()
        .LoadAssemblies(
            typeof(MySite.Domain.Content.Locale).Assembly,
            typeof(MySite.Application.Content.IContentService).Assembly,
            typeof(MySite.Contracts.Content.HomeResponse).Assembly,
            typeof(MySite.Infrastructure.Postgres.Persistence.MySiteDbContext).Assembly,
            typeof(MySite.Web.DependencyInjectionExtension).Assembly,
            typeof(MySite.Domain.NegativeFixture.LeakyDomainType).Assembly)
        .Build();
}
