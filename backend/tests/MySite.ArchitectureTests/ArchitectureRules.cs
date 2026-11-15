using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;

namespace MySite.ArchitectureTests;

/// <summary>
/// The project's architecture rules, expressed once as rule objects. The production check and the
/// negative-fixture check run the same instances: a rule that has never been seen to fail is not a
/// check.
/// </summary>
internal static class ArchitectureRules
{
    // --- Providers (declared before the rules that use them: static initializers run top-down) ---

    private static readonly IObjectProvider<IType> DomainLayer =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite\\.Domain(\\..*)?$").As("the Domain layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite\\.Application(\\..*)?$").As("the Application layer");

    private static readonly IObjectProvider<IType> ContractsLayer =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite\\.Contracts(\\..*)?$").As("the Contracts layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite\\.Infrastructure\\.Postgres(\\..*)?$")
            .As("the Infrastructure layer");

    private static readonly IObjectProvider<IType> WebLayer =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite\\.Web(\\..*)?$").As("the Web layer");

    private static readonly IObjectProvider<IType> BaseClassLibrary =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^System(\\..*)?$").As("the base class library");

    private static readonly IObjectProvider<IType> AllMySiteTypes =
        ArchRuleDefinition.Types().That().ResideInNamespaceMatching("^MySite(\\..*)?$").As("all MySite types");

    /// <summary>The Domain layer together with the base class library, the only two things it may touch.</summary>
    private static readonly IObjectProvider<IType> DomainAndBaseClassLibrary =
        ArchRuleDefinition.Types().That().Are(DomainLayer).Or().Are(BaseClassLibrary)
            .As("the Domain layer and the base class library");

    /// <summary>The Contracts layer together with the base class library.</summary>
    private static readonly IObjectProvider<IType> ContractsAndBaseClassLibrary =
        ArchRuleDefinition.Types().That().Are(ContractsLayer).Or().Are(BaseClassLibrary)
            .As("the Contracts layer and the base class library");

    // --- Rules ---

    internal static readonly IArchRule DomainDependsOnlyOnItselfAndTheBaseClassLibrary =
        ArchRuleDefinition.Types().That().Are(DomainLayer)
            .Should().OnlyDependOn(DomainAndBaseClassLibrary)
            .Because("domain types must not depend on higher layers");

    internal static readonly IArchRule ApplicationDoesNotDependOnInfrastructureOrWeb =
        ArchRuleDefinition.Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(WebLayer)
            .Because("application layer must not reach infrastructure or web");

    internal static readonly IArchRule InfrastructureDoesNotDependOnWeb =
        ArchRuleDefinition.Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(WebLayer)
            .Because("infrastructure must not depend on the web layer");

    internal static readonly IArchRule ContractsDependsOnlyOnTheBaseClassLibrary =
        ArchRuleDefinition.Types().That().Are(ContractsLayer)
            .Should().OnlyDependOn(ContractsAndBaseClassLibrary)
            .Because("contract types are pure DTOs and must depend on nothing but BCL");

    internal static readonly IArchRule NoAsyncVoidMethod =
        ArchRuleDefinition.MethodMembers().That().AreDeclaredIn(AllMySiteTypes)
            .Should().FollowCustomCondition(IsNotAsyncVoid, "not be async void",
                "async void cannot be awaited and its exceptions crash the process; return Task instead")
            .Because("async void methods cannot be awaited and their exceptions crash the process");

    internal static readonly IArchRule InterfacesStartWithI =
        ArchRuleDefinition.Interfaces().That().ResideInNamespaceMatching("^MySite(\\..*)?$")
            .Should().HaveNameStartingWith("I")
            .Because("all MySite interfaces should start with 'I' to follow C# conventions");

    internal static readonly IArchRule ControllersEndWithController =
        ArchRuleDefinition.Classes().That().ResideInNamespaceMatching("^MySite\\.Web\\.Controllers(\\..*)?$")
            .Should().HaveNameEndingWith("Controller")
            .Because("Web controllers should end with 'Controller' to follow ASP.NET Core conventions");

    internal static readonly IArchRule RepositoryAdaptersEndWithTheirRole =
        ArchRuleDefinition.Classes().That().ResideInNamespaceMatching("^MySite\\.Infrastructure\\.Postgres\\.Persistence\\.Repositories(\\..*)?$")
            .Should().HaveNameMatching(".*(Repository|Writer)$")
            .Because("repository and writer adapters should end with their role name");

    private static bool IsNotAsyncVoid(MethodMember method)
    {
        if (!string.Equals(method.ReturnType.FullName, "System.Void", StringComparison.Ordinal))
        {
            return true;
        }

        foreach (var attribute in method.AttributeInstances)
        {
            if (string.Equals(
                    attribute.Type.FullName,
                    "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
