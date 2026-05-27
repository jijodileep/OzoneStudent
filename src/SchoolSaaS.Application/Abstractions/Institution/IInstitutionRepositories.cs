using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Application.Abstractions.Institution;

public interface IGradeRepository
{
    Task<IReadOnlyList<Grade>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Grade?> GetByIdAsync(Guid tenantId, Guid gradeId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid tenantId,
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<Grade> AddAsync(Grade grade, CancellationToken cancellationToken = default);
}

public interface IClassRepository
{
    Task<IReadOnlyList<SchoolClass>> ListAsync(
        Guid tenantId,
        Guid? gradeId = null,
        Guid? academicYearId = null,
        CancellationToken cancellationToken = default);

    Task<SchoolClass?> GetByIdAsync(Guid tenantId, Guid classId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid tenantId,
        Guid gradeId,
        Guid academicYearId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<SchoolClass> AddAsync(SchoolClass schoolClass, CancellationToken cancellationToken = default);

    Task<Section?> GetSectionByIdAsync(Guid tenantId, Guid sectionId, CancellationToken cancellationToken = default);

    Task<bool> SectionExistsByNameAsync(
        Guid tenantId,
        Guid classId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<Section> AddSectionAsync(Section section, CancellationToken cancellationToken = default);
}

public interface IStaffRepository
{
    Task<IReadOnlyList<Staff>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Staff?> GetByIdAsync(Guid tenantId, Guid staffId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmployeeCodeAsync(
        Guid tenantId,
        string employeeCode,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserLinkedAsync(
        Guid tenantId,
        Guid userId,
        Guid? excludeStaffId = null,
        CancellationToken cancellationToken = default);

    Task<Staff> AddAsync(Staff staff, CancellationToken cancellationToken = default);

    Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default);
}

public interface IInstitutionReferenceRepository
{
    Task<bool> AcademicYearExistsAsync(Guid tenantId, Guid academicYearId, CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
