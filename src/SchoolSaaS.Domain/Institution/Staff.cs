using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Identity;

namespace SchoolSaaS.Domain.Institution;

public sealed class Staff : BaseEntity
{
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public StaffStatus Status { get; set; } = StaffStatus.Active;

    public DateOnly JoinDate { get; set; }
}
