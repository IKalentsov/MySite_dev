using ArchUnitNET.xUnitV3;
using Xunit;

namespace MySite.ArchitectureTests;

public sealed class ConventionTests
{
    [Fact]
    public void NoAsyncVoidMethod() =>
        ArchitectureRules.NoAsyncVoidMethod.Check(TestArchitectures.Production);

    [Fact]
    public void InterfacesStartWithI() =>
        ArchitectureRules.InterfacesStartWithI.Check(TestArchitectures.Production);

    [Fact]
    public void ControllersEndWithController() =>
        ArchitectureRules.ControllersEndWithController.Check(TestArchitectures.Production);

    [Fact]
    public void RepositoryAdaptersEndWithTheirRole() =>
        ArchitectureRules.RepositoryAdaptersEndWithTheirRole.Check(TestArchitectures.Production);
}
