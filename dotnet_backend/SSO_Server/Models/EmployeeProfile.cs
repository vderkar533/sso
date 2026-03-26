namespace SSO.Server.Models;

public sealed class EmployeeProfile
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? GradeId { get; set; }
    public Grade? Grade { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string AadharNumber { get; set; } = string.Empty;
}
