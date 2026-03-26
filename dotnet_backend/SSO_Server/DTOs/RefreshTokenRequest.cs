using System.ComponentModel.DataAnnotations;

namespace SSO.Server.DTOs;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string? ClientId { get; set; }
}
