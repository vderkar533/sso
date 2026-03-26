using System.ComponentModel.DataAnnotations;

namespace SSO.Server.DTOs;

public sealed class SendEmailVerificationRequest
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
