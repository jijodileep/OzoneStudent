using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Institution;

namespace SchoolSaaS.Infrastructure.Institution;

public static class InstitutionDependencyInjection
{
    public static IServiceCollection AddInstitutionInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IInstitutionReferenceRepository, InstitutionReferenceRepository>();
        services.AddScoped<ICustomFieldRepository, CustomFieldRepository>();
        services.AddScoped<IProfileDocumentRepository, ProfileDocumentRepository>();
        services.AddScoped<IProfileDocumentOwnerValidator, ProfileDocumentOwnerValidator>();
        services.AddScoped<CustomFieldValueService>();
        services.AddScoped<ProfileDocumentService>();
        return services;
    }
}
