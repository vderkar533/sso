using SSO.Server.DTOs;
using SSO.Server.Models;

namespace SSO.Server.TokenService;

public interface ITokenService
{
    AuthResponse CreateTokens(ApplicationUser user, Grade? grade);
    string GenerateSecureToken(int size = 64);
}
