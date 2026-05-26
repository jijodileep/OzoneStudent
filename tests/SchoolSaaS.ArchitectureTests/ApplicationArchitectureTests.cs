using System.Reflection;
using FluentValidation;
using NetArchTest.Rules;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.ArchitectureTests;

public class ApplicationArchitectureTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(SchoolSaaS.Application.AssemblyMarker).Assembly;

    [Fact]
    public void Handlers_ShouldResideInApplicationLayer()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespace("SchoolSaaS.Application")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Commands_ShouldHaveFluentValidationValidator()
    {
        var commandTypes = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                && t.GetInterfaces().Any(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            .ToList();

        var missingValidators = commandTypes
            .Where(commandType =>
            {
                var validatorName = commandType.Name + "Validator";
                return !ApplicationAssembly.GetTypes().Any(t =>
                    t.Name == validatorName
                    && t.GetInterfaces().Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IValidator<>)
                        && i.GetGenericArguments()[0] == commandType));
            })
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            missingValidators.Count == 0,
            "Commands missing validators: " + string.Join(", ", missingValidators));
    }

    [Fact]
    public void Commands_ShouldHaveRequirePermissionOrAllowAnonymous()
    {
        var commandTypes = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                && t.GetInterfaces().Any(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            .ToList();

        var missing = commandTypes
            .Where(t =>
                !typeof(IAllowAnonymousCommand).IsAssignableFrom(t)
                && !t.GetCustomAttributes<RequirePermissionAttribute>(inherit: true).Any())
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            missing.Count == 0,
            "Commands missing [RequirePermission] or IAllowAnonymousCommand: " + string.Join(", ", missing));
    }

    [Fact]
    public void Infrastructure_ShouldNotReferenceApi()
    {
        var result = Types.InAssembly(typeof(SchoolSaaS.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SchoolSaaS.Api")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Shared_ShouldNotReferenceInfrastructureOrApplication()
    {
        var result = Types.InAssembly(typeof(SchoolSaaS.Shared.Results.Result).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("SchoolSaaS.Infrastructure", "SchoolSaaS.Application", "SchoolSaaS.Api")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}
