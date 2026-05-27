using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Application.Abstractions.Institution;

public interface IAcademicYearRepository
{
    Task<IReadOnlyList<AcademicYear>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AcademicYear> Items, int TotalCount)> ListPagedAsync(
        Guid tenantId,
        AcademicYearListFilter filter,
        CancellationToken cancellationToken = default);

    Task<AcademicYear?> GetByIdAsync(
        Guid tenantId,
        Guid academicYearId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid tenantId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingDatesAsync(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<AcademicYear> AddAsync(AcademicYear academicYear, CancellationToken cancellationToken = default);

    Task SetCurrentAsync(Guid tenantId, Guid academicYearId, CancellationToken cancellationToken = default);
}
