using SSO.Server.Models;

namespace SSO.Server.Data;

public static class ApplicationSeedData
{
    public static readonly ApplicationClient[] All =
    [
        new ApplicationClient
        {
            ApplicationClientId = Guid.Parse("22222222-2222-2222-2222-222222222221"),
            ClientId = "internal-portal",
            ClientName = "Internal Web Portal",
            RedirectUri = "https://portal.yourcompany.com/signin-oidc",
            IsActive = true
        },
        new ApplicationClient
        {
            ApplicationClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ClientId = "mobile-app",
            ClientName = "Android Mobile App",
            RedirectUri = "com.yourcompany.app://callback",
            IsActive = true
        }
    ];
}
