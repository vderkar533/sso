using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSO.Server.Configurations;
using SSO.Server.DTOs;
using SSO.Server.Models;

namespace SSO.Server.TokenService;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public AuthResponse CreateTokens(ApplicationUser user, Grade? grade)
    {
        var accessToken = CreateJwtToken(user, grade, "access");
        var idToken = CreateJwtToken(user, grade, "id");

        return new AuthResponse
        {
            AccessToken = accessToken,
            IdToken = idToken,
            ExpiresIn = _settings.AccessTokenMinutes * 60
        };
    }

    public string GenerateSecureToken(int size = 64)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(size));
    }

    private string CreateJwtToken(ApplicationUser user, Grade? grade, string tokenType)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTimeOffset.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.EmployeeId),
            new("user_id", user.EmployeeId),
            new("employee_id", user.EmployeeId),
            new("name", user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("department", user.Department?.DepartmentName ?? string.Empty),
            new("grade", grade?.GradeCode ?? string.Empty),
            new("hierarchy_level", grade?.HierarchyLevel.ToString() ?? string.Empty),
            new("user_type", user.UserType.ToString()),
            new("typ", tokenType)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_settings.AccessTokenMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
