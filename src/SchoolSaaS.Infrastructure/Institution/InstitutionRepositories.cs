using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Institution;

public sealed class GradeRepository(ApplicationDbContext db) : IGradeRepository
{
    public async Task<IReadOnlyList<Grade>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await db.Grades.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Grade?> GetByIdAsync(
        Guid tenantId,
        Guid gradeId,
        CancellationToken cancellationToken = default) =>
        db.Grades.AsNoTracking()
            .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == gradeId,
            cancellationToken);

    public Task<bool> ExistsByCodeAsync(
        Guid tenantId,
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.Grades.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.Code == code
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public async Task<Grade> AddAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        db.Grades.Add(grade);
        await db.SaveChangesAsync(cancellationToken);
        return grade;
    }
}

public sealed class ClassRepository(ApplicationDbContext db) : IClassRepository
{
    public async Task<IReadOnlyList<SchoolClass>> ListAsync(
        Guid tenantId,
        Guid? gradeId = null,
        Guid? academicYearId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Classes.AsNoTracking()
            .Include(x => x.Grade)
            .Include(x => x.Sections)
            .Where(x => x.TenantId == tenantId);

        if (gradeId is not null)
        {
            query = query.Where(x => x.GradeId == gradeId);
        }

        if (academicYearId is not null)
        {
            query = query.Where(x => x.AcademicYearId == academicYearId);
        }

        return await query
            .OrderBy(x => x.Grade!.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<SchoolClass?> GetByIdAsync(
        Guid tenantId,
        Guid classId,
        CancellationToken cancellationToken = default) =>
        db.Classes
            .Include(x => x.Grade)
            .Include(x => x.Sections)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == classId, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        Guid tenantId,
        Guid gradeId,
        Guid academicYearId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.Classes.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.GradeId == gradeId
                     && x.AcademicYearId == academicYearId
                     && x.Name == name
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public async Task<SchoolClass> AddAsync(
        SchoolClass schoolClass,
        CancellationToken cancellationToken = default)
    {
        db.Classes.Add(schoolClass);
        await db.SaveChangesAsync(cancellationToken);
        return schoolClass;
    }

    public Task<Section?> GetSectionByIdAsync(
        Guid tenantId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        db.Sections.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == sectionId,
            cancellationToken);

    public Task<bool> SectionExistsByNameAsync(
        Guid tenantId,
        Guid classId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.Sections.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.ClassId == classId
                     && x.Name == name
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public async Task<Section> AddSectionAsync(
        Section section,
        CancellationToken cancellationToken = default)
    {
        db.Sections.Add(section);
        await db.SaveChangesAsync(cancellationToken);
        return section;
    }
}

public sealed class StaffRepository(ApplicationDbContext db) : IStaffRepository
{
    public async Task<IReadOnlyList<Staff>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await db.StaffMembers.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.EmployeeCode)
            .ToListAsync(cancellationToken);

    public Task<Staff?> GetByIdAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default) =>
        db.StaffMembers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == staffId,
            cancellationToken);

    public Task<bool> ExistsByEmployeeCodeAsync(
        Guid tenantId,
        string employeeCode,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.StaffMembers.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.EmployeeCode == employeeCode
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public Task<bool> IsUserLinkedAsync(
        Guid tenantId,
        Guid userId,
        Guid? excludeStaffId = null,
        CancellationToken cancellationToken = default) =>
        db.StaffMembers.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.UserId == userId
                     && (excludeStaffId == null || x.Id != excludeStaffId),
                cancellationToken);

    public async Task<Staff> AddAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        db.StaffMembers.Add(staff);
        await db.SaveChangesAsync(cancellationToken);
        return staff;
    }

    public async Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        db.StaffMembers.Update(staff);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class InstitutionReferenceRepository(ApplicationDbContext db) : IInstitutionReferenceRepository
{
    public Task<bool> AcademicYearExistsAsync(
        Guid tenantId,
        Guid academicYearId,
        CancellationToken cancellationToken = default) =>
        db.AcademicYears.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == academicYearId, cancellationToken);

    public Task<bool> UserExistsAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == userId, cancellationToken);
}
