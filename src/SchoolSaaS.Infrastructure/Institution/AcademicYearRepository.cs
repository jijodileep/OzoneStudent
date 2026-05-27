using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Institution;

public sealed class AcademicYearRepository(ApplicationDbContext db) : IAcademicYearRepository
{
    public async Task<IReadOnlyList<AcademicYear>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        (await ListPagedAsync(
            tenantId,
            new AcademicYearListFilter(Page: 1, PageSize: int.MaxValue),
            cancellationToken)).Items;

    public async Task<(IReadOnlyList<AcademicYear> Items, int TotalCount)> ListPagedAsync(
        Guid tenantId,
        AcademicYearListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = db.AcademicYears.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term)
                || x.Status.ToString().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status)
            && Enum.TryParse<AcademicYearStatus>(filter.Status, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (filter.IsCurrent is bool isCurrent)
        {
            query = query.Where(x => x.IsCurrent == isCurrent);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = ApplySort(query, filter);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<AcademicYear> ApplySort(IQueryable<AcademicYear> query, AcademicYearListFilter filter)
    {
        var descending = filter.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (filter.SortBy?.ToLowerInvariant()) switch
        {
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "startdate" => descending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
            "enddate" => descending ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "iscurrent" => descending ? query.OrderByDescending(x => x.IsCurrent) : query.OrderBy(x => x.IsCurrent),
            _ => query.OrderByDescending(x => x.StartDate),
        };
    }

    public Task<AcademicYear?> GetByIdAsync(
        Guid tenantId,
        Guid academicYearId,
        CancellationToken cancellationToken = default) =>
        db.AcademicYears.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == academicYearId,
            cancellationToken);

    public Task<bool> ExistsByNameAsync(
        Guid tenantId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.AcademicYears.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.Name == name
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public Task<bool> HasOverlappingDatesAsync(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.AcademicYears.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && (excludeId == null || x.Id != excludeId)
                     && x.StartDate <= endDate
                     && startDate <= x.EndDate,
                cancellationToken);

    public async Task<AcademicYear> AddAsync(
        AcademicYear academicYear,
        CancellationToken cancellationToken = default)
    {
        db.AcademicYears.Add(academicYear);
        await db.SaveChangesAsync(cancellationToken);
        return academicYear;
    }

    public async Task SetCurrentAsync(
        Guid tenantId,
        Guid academicYearId,
        CancellationToken cancellationToken = default)
    {
        var years = await db.AcademicYears
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var target = years.FirstOrDefault(x => x.Id == academicYearId)
            ?? throw new InvalidOperationException($"Academic year {academicYearId} was not found.");

        foreach (var year in years)
        {
            year.IsCurrent = year.Id == academicYearId;
            if (year.IsCurrent)
            {
                year.Status = AcademicYearStatus.Active;
            }
            else if (year.Status == AcademicYearStatus.Active)
            {
                year.Status = year.EndDate < DateOnly.FromDateTime(DateTime.UtcNow)
                    ? AcademicYearStatus.Closed
                    : AcademicYearStatus.Upcoming;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
