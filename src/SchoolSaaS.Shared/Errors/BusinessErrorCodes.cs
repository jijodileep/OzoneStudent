using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Shared.Errors;

public static class BusinessErrorCodes
{
    public static bool IsConflict(string code) =>
        code == ErrorFactory.ConflictCode || ConflictCodes.Contains(code);

    private static readonly HashSet<string> ConflictCodes = new(StringComparer.Ordinal)
    {
        InstitutionErrorCodes.AcademicYear.NameExists,
        InstitutionErrorCodes.AcademicYear.DatesOverlap,
    };
}
