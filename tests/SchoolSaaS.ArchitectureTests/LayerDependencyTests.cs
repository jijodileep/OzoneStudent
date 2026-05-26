using NetArchTest.Rules;

namespace SchoolSaaS.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldNotReferenceInfrastructureApplicationOrApi()
    {
        var result = Types.InAssembly(typeof(SchoolSaaS.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SchoolSaaS.Infrastructure",
                "SchoolSaaS.Application",
                "SchoolSaaS.Api")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(SchoolSaaS.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("SchoolSaaS.Infrastructure", "SchoolSaaS.Api")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}
