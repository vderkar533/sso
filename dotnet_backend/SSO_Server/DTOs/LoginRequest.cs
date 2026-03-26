using System.ComponentModel.DataAnnotations;

namespace SSO.Server.DTOs;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ClientId { get; set; }
}
