using ArchUnitNET.xUnitV3;
using Xunit;

namespace MySite.ArchitectureTests;

/// <summary>
/// These tests prove every rule still catches a violation; a rule that has never been seen to fail is not a check.
/// </summary>
public sealed class NegativeFixtureTests
{
    [Fact]
    public void DomainDependencyRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.DomainDependsOnlyOnItselfAndTheBaseClassLibrary
                .Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void ApplicationDependencyRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.ApplicationDoesNotDependOnInfrastructureOrWeb
                .Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void InfrastructureDependencyRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.InfrastructureDoesNotDependOnWeb
                .Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void ContractsDependencyRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.ContractsDependsOnlyOnTheBaseClassLibrary
                .Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void AsyncVoidRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.NoAsyncVoidMethod.Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void InterfaceNamingRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.InterfacesStartWithI.Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void ControllerNamingRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.ControllersEndWithController.Check(TestArchitectures.WithNegativeFixture));

    [Fact]
    public void RepositoryNamingRuleFailsOnTheNegativeFixture() =>
        Assert.Throws<FailedArchRuleException>(() =>
            ArchitectureRules.RepositoryAdaptersEndWithTheirRole.Check(TestArchitectures.WithNegativeFixture));
}
