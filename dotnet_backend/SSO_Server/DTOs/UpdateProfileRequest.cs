using System.ComponentModel.DataAnnotations;
using SSO.Server.Models;

namespace SSO.Server.DTOs;

public sealed class UpdateProfileRequest
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int GradeId { get; set; }

    [Required]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string AadharNumber { get; set; } = string.Empty;

    [Required]
    public UserType UserType { get; set; }

    public string? ContractorAgencyName { get; set; }
}
