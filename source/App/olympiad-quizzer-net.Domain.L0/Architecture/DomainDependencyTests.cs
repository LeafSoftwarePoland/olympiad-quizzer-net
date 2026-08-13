using System.Reflection;
using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Domain.L0.Architecture;

[Trait("Tier", "L0")]
public sealed class DomainDependencyTests
{
    [Fact]
    public void DomainAssembly_ReferencesOnlyFrameworkAssemblies_ReturnsTrue()
    {
        Assembly domain = typeof(Question).Assembly;
        AssemblyName[] referenced = domain.GetReferencedAssemblies();

        List<string> violations = referenced
            .Select(a => a.Name)
            .Where(name => !IsAllowed(name))
            .ToList();

        Assert.True(
            violations.Count == 0,
            $"Domain references non-framework assemblies: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainAssembly_ContainsNoTypeNamedRepositoryImplementation_ReturnsTrue()
    {
        Assembly domain = typeof(Question).Assembly;
        Type[] allTypes = domain.GetTypes();

        List<string> repositoryViolations = allTypes
            .Where(t => t.IsPublic && t.Name.EndsWith("Repository", StringComparison.Ordinal) && !t.IsInterface)
            .Select(t => t.FullName)
            .ToList();

        List<string> abstractionsViolations = allTypes
            .Where(t => t.Namespace == "OlympiadQuizzer.Domain.Abstractions" && !t.IsInterface)
            .Select(t => t.FullName)
            .ToList();

        List<string> allViolations = repositoryViolations.Concat(abstractionsViolations).Distinct().ToList();

        Assert.True(
            allViolations.Count == 0,
            $"Domain Abstractions namespace or Repository-named types contain non-interface types: {string.Join(", ", allViolations)}");
    }

    private static bool IsAllowed(string assemblyName)
    {
        return assemblyName == "netstandard"
            || assemblyName == "mscorlib"
            || assemblyName == "System"
            || assemblyName.StartsWith("System.", StringComparison.Ordinal);
    }
}
