namespace SSO.Server.Models;

public sealed class RefreshToken
{
    public Guid RefreshTokenId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public Guid? ApplicationClientId { get; set; }
    public ApplicationClient? ApplicationClient { get; set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow <= ExpiresAt;
}
