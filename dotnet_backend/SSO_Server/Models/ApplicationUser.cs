namespace SSO.Server.Models;

public sealed class ApplicationUser
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool EmailConfirmed { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? GradeId { get; set; }
    public Grade? Grade { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string AadharNumber { get; set; } = string.Empty;
    public UserType UserType { get; set; }

    // Email verification flow fields.
    public bool EmailVerified { get; set; }
}
