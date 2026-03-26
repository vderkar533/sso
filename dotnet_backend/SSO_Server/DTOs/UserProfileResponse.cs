namespace SSO.Server.DTOs;

public sealed class UserProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string Department { get; set; } = string.Empty;
    public int? GradeId { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string GradeTitle { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public string UserType { get; set; } = string.Empty;
    public int UserTypeId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string AadharNumber { get; set; } = string.Empty;
    public string ContractorAgencyName { get; set; } = string.Empty;
}
