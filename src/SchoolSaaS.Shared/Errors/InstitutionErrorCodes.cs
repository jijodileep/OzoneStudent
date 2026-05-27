namespace SchoolSaaS.Shared.Errors;

public static class InstitutionErrorCodes
{
    public const string TenantRequired = "institution.tenant_required";

    public static class AcademicYear
    {
        public const string NameRequired = "institution.academic_year.name_required";
        public const string NameTooLong = "institution.academic_year.name_too_long";
        public const string StartDateRequired = "institution.academic_year.start_date_required";
        public const string EndDateRequired = "institution.academic_year.end_date_required";
        public const string EndDateBeforeStart = "institution.academic_year.end_date_before_start";
        public const string NameExists = "institution.academic_year.name_exists";
        public const string DatesOverlap = "institution.academic_year.dates_overlap";
    }
}
