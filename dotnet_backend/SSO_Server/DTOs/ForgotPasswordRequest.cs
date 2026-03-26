using System.ComponentModel.DataAnnotations;

namespace SSO.Server.DTOs;

public sealed class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
