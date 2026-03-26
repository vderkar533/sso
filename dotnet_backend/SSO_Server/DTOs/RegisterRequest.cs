using System.ComponentModel.DataAnnotations;
using SSO.Server.Models;

namespace SSO.Server.DTOs;

public sealed class RegisterRequest
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    [RegularExpression(@"(?i)^[A-Z0-9._%+-]+@lloyds\.in$", ErrorMessage = "Email must be a @lloyds.in address.")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int GradeId { get; set; }

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhar number must be exactly 12 digits.")]
    public string AadharNumber { get; set; } = string.Empty;

    [Required]
    public UserType UserType { get; set; }

    public string? ContractorAgencyName { get; set; }
}
