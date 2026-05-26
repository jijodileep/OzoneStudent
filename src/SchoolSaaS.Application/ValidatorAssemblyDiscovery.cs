using System.Reflection;

namespace SchoolSaaS.Application;

public static class ValidatorAssemblyDiscovery
{
    public static IReadOnlyList<Assembly> Discover()
    {
        var assemblies = new List<Assembly> { typeof(AssemblyMarker).Assembly };

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name is null
                || assembly.IsDynamic
                || !name.StartsWith("SchoolSaaS.", StringComparison.Ordinal)
                || !name.EndsWith(".Application", StringComparison.Ordinal)
                || assemblies.Contains(assembly))
            {
                continue;
            }

            assemblies.Add(assembly);
        }

        return assemblies;
    }
}
