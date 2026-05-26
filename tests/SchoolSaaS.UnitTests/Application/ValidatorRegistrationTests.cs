using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Application;
using SchoolSaaS.Application.Platform.Queries;
using SchoolSaaS.Application.Platform.Validators;

namespace SchoolSaaS.UnitTests.Application;

public class ValidatorRegistrationTests
{
    [Fact]
    public void AddApplication_RegistersValidatorsFromApplicationAssembly()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var validators = provider.GetServices<IValidator<PingQuery>>();

        validators.Should().ContainSingle(v => v is PingQueryValidator);
    }

    [Fact]
    public void ValidatorAssemblyDiscovery_IncludesApplicationAssembly()
    {
        var assemblies = ValidatorAssemblyDiscovery.Discover();

        assemblies.Should().Contain(a => a == typeof(AssemblyMarker).Assembly);
    }
}
