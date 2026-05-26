namespace SchoolSaaS.ArchitectureTests;

public class SolutionStructureTests
{
    [Fact]
    public void Domain_Assembly_IsLoadable()
    {
        var assembly = typeof(SchoolSaaS.Domain.AssemblyMarker).Assembly;

        Assert.Equal("SchoolSaaS.Domain", assembly.GetName().Name);
    }
}
