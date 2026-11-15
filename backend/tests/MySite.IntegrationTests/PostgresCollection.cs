using Xunit;

namespace MySite.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "postgres";
}
