using SSO.Server.DTOs;

namespace SSO.Server.Services;

public interface IAuthService
{
    Task<bool> SendEmailVerificationAsync(string employeeId, string email, CancellationToken cancellationToken);
    Task<bool> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken cancellationToken);
    Task<bool> LogoutAsync(LogoutRequest request, string ipAddress, CancellationToken cancellationToken);
    Task<bool> SendPasswordResetAsync(string email, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
