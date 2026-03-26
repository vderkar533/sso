using System.ComponentModel.DataAnnotations;

namespace SSO.Server.DTOs;

public sealed class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
