namespace SSO.Server.DTOs;

public sealed class AuthUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? GradeId { get; set; }
    public string GradeCode { get; set; } = string.Empty;
    public string GradeTitle { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string AadharNumber { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string ContractorAgencyName { get; set; } = string.Empty;
}
