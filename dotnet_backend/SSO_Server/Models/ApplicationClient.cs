namespace SSO.Server.Models;

public sealed class ApplicationClient
{
    public Guid ApplicationClientId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
