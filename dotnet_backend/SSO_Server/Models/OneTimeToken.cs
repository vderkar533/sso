namespace SSO.Server.Models;

public sealed class OneTimeToken
{
    public Guid OneTimeTokenId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public TokenPurpose Purpose { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
