using ArchUnitNET.xUnitV3;
using Xunit;

namespace MySite.ArchitectureTests;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void DomainDependsOnlyOnItselfAndTheBaseClassLibrary() =>
        ArchitectureRules.DomainDependsOnlyOnItselfAndTheBaseClassLibrary
            .Check(TestArchitectures.Production);

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructureOrWeb() =>
        ArchitectureRules.ApplicationDoesNotDependOnInfrastructureOrWeb
            .Check(TestArchitectures.Production);

    [Fact]
    public void InfrastructureDoesNotDependOnWeb() =>
        ArchitectureRules.InfrastructureDoesNotDependOnWeb
            .Check(TestArchitectures.Production);

    [Fact]
    public void ContractsDependsOnlyOnTheBaseClassLibrary() =>
        ArchitectureRules.ContractsDependsOnlyOnTheBaseClassLibrary
            .Check(TestArchitectures.Production);
}
