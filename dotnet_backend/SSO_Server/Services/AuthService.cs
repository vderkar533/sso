using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSO.Server.Data;
using SSO.Server.DTOs;
using SSO.Server.EmailService;
using SSO.Server.Models;
using SSO.Server.Repositories;
using SSO.Server.TokenService;
using Microsoft.Extensions.Options;
using SSO.Server.Configurations;

namespace SSO.Server.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _publicBaseUrl;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEmailService emailService,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationRepository applicationRepository,
        IOptions<JwtSettings> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _applicationRepository = applicationRepository;
        _jwtSettings = jwtOptions.Value;
        _httpContextAccessor = httpContextAccessor;
        _publicBaseUrl = configuration["PublicBaseUrl"];
    }

    public async Task<bool> SendEmailVerificationAsync(string employeeId, string email, CancellationToken cancellationToken)
    {
        // Step 1 of the email verification flow: generate and email a one-time token.
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        var employeeIdOwner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Trim(), cancellationToken);

        if (employeeIdOwner is not null && !string.Equals(employeeIdOwner.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (user is not null && user.EmailVerified)
        {
            return false;
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                EmployeeId = employeeId.Trim(),
                UserName = email,
                Email = normalizedEmail,
                EmailVerified = false,
                EmailConfirmed = false
            };

            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(user.EmployeeId, employeeId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var token = _tokenService.GenerateSecureToken();
        await ReplaceOneTimeTokenAsync(user.EmployeeId, TokenPurpose.EmailVerification, token, DateTimeOffset.UtcNow.AddHours(24), cancellationToken);

        var returnUrl = $"lloyds-sso://verify?email={Uri.EscapeDataString(email)}";
        var verifyLink =
            $"{ResolvePublicBaseUrl()}/api/auth/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        var body = $@"
<p>Click the button below to verify your email and continue registration.</p>
<p><a href=""{verifyLink}"">Verify Email</a></p>";

        await _emailService.SendEmailAsync(email, "Verify your email", body, cancellationToken);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        // Step 2 of the email verification flow: validate token and mark email verified.
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var tokenHash = HashToken(token);
        var stored = await _dbContext.OneTimeTokens
            .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId &&
                                      x.Purpose == TokenPurpose.EmailVerification &&
                                      x.TokenHash == tokenHash, cancellationToken);
        if (stored is null || stored.ExpiresAt < DateTimeOffset.UtcNow)
        {
            if (stored is not null)
            {
                _dbContext.OneTimeTokens.Remove(stored);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return false;
        }

        user.EmailVerified = true;
        user.EmailConfirmed = true;
        _dbContext.OneTimeTokens.Remove(stored);

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        // Step 3 of the email verification flow: complete registration after email is verified.
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        var employeeIdOwner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId.Trim(), cancellationToken);

        if (employeeIdOwner is not null && !string.Equals(employeeIdOwner.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Employee ID is already registered with a different email.");
        }
        if (user is not null && !string.Equals(user.EmployeeId, request.EmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email is already registered with a different employee ID.");
        }
        if (user is null)
        {
            user = new ApplicationUser
            {
                EmployeeId = request.EmployeeId.Trim(),
                UserName = request.Email,
                Email = normalizedEmail,
                EmailVerified = false,
                EmailConfirmed = false
            };

            user.EmployeeId = request.EmployeeId;
            user.Name = request.Name;
            user.DepartmentId = request.DepartmentId;
            user.GradeId = request.GradeId;
            user.MobileNumber = request.MobileNumber;
            user.AadharNumber = request.AadharNumber;
            user.UserType = request.UserType;

            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException("Email must be verified before registration.");
        }
        if (!string.Equals(user.EmployeeId, request.EmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Employee ID does not match this email.");
        }

        if (!user.EmailVerified)
        {
            user.EmployeeId = request.EmployeeId;
            user.Name = request.Name;
            user.DepartmentId = request.DepartmentId;
            user.GradeId = request.GradeId;
            user.MobileNumber = request.MobileNumber;
            user.AadharNumber = request.AadharNumber;
            user.UserType = request.UserType;

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = request.Email;
            }

            user.Email = normalizedEmail;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Email must be verified before registration.");
        }
        if (user.PasswordHash is not null)
        {
            throw new InvalidOperationException("User is already registered.");
        }

        var grade = await _dbContext.Grades.FirstOrDefaultAsync(x => x.GradeId == request.GradeId, cancellationToken);
        if (grade is null)
        {
            throw new InvalidOperationException("Invalid grade.");
        }

        var department = await _dbContext.Departments.FirstOrDefaultAsync(x => x.DepartmentId == request.DepartmentId, cancellationToken);
        if (department is null)
        {
            throw new InvalidOperationException("Invalid department.");
        }

        user.EmployeeId = request.EmployeeId;
        user.Name = request.Name;
        user.DepartmentId = request.DepartmentId;
        user.Department = department;
        user.GradeId = request.GradeId;
        user.MobileNumber = request.MobileNumber;
        user.AadharNumber = request.AadharNumber;
        user.UserType = request.UserType;

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user.Email = normalizedEmail;
        _dbContext.Users.Update(user);

        if (request.UserType == UserType.Contractor && string.IsNullOrWhiteSpace(request.ContractorAgencyName))
        {
            throw new InvalidOperationException("Contractor agency name is required.");
        }

        if (request.UserType == UserType.Employee)
        {
            var profile = await _dbContext.EmployeeProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
            if (profile is null)
            {
                profile = new EmployeeProfile { UserId = user.EmployeeId };
                await _dbContext.EmployeeProfiles.AddAsync(profile, cancellationToken);
            }

            profile.EmployeeId = request.EmployeeId;
            profile.Name = request.Name;
            profile.DepartmentId = request.DepartmentId;
            profile.GradeId = request.GradeId;
            profile.MobileNumber = request.MobileNumber;
            profile.AadharNumber = request.AadharNumber;
        }
        else
        {
            var profile = await _dbContext.ContractorProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
            if (profile is null)
            {
                profile = new ContractorProfile { UserId = user.EmployeeId };
                await _dbContext.ContractorProfiles.AddAsync(profile, cancellationToken);
            }

            profile.EmployeeId = request.EmployeeId;
            profile.Name = request.Name;
            profile.DepartmentId = request.DepartmentId;
            profile.GradeId = request.GradeId;
            profile.MobileNumber = request.MobileNumber;
            profile.AadharNumber = request.AadharNumber;
            profile.AgencyName = request.ContractorAgencyName?.Trim() ?? string.Empty;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = _tokenService.CreateTokens(user, grade);
        var refreshToken = await CreateRefreshTokenAsync(user, "registration", null, cancellationToken);
        response.RefreshToken = refreshToken.Token;
        response.User = new AuthUserInfo
        {
            UserId = user.EmployeeId,
            EmployeeId = user.EmployeeId,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            DepartmentId = user.DepartmentId,
            DepartmentName = department.DepartmentName,
            GradeId = user.GradeId,
            GradeCode = grade.GradeCode,
            GradeTitle = grade.Title,
            MobileNumber = user.MobileNumber,
            AadharNumber = user.AadharNumber,
            UserType = user.UserType.ToString(),
            EmailVerified = user.EmailVerified,
            ContractorAgencyName = request.UserType == UserType.Contractor
                ? request.ContractorAgencyName?.Trim() ?? string.Empty
                : string.Empty
        };
        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _dbContext.Users
            .Include(x => x.Grade)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.EmailVerified)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }
        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        ApplicationClient? applicationClient = null;
        if (!string.IsNullOrWhiteSpace(request.ClientId))
        {
            applicationClient = await _applicationRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
            if (applicationClient is null)
            {
                throw new InvalidOperationException("Invalid client.");
            }
        }

        var response = _tokenService.CreateTokens(user, user.Grade);
        var refreshToken = await CreateRefreshTokenAsync(user, ipAddress, applicationClient, cancellationToken);
        response.RefreshToken = refreshToken.Token;

        var contractorAgencyName = string.Empty;
        if (user.UserType == UserType.Contractor)
        {
            contractorAgencyName = await _dbContext.ContractorProfiles
                .AsNoTracking()
                .Where(x => x.UserId == user.EmployeeId)
                .Select(x => x.AgencyName)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        }

        response.User = new AuthUserInfo
        {
            UserId = user.EmployeeId,
            EmployeeId = user.EmployeeId,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.DepartmentName ?? string.Empty,
            GradeId = user.GradeId,
            GradeCode = user.Grade?.GradeCode ?? string.Empty,
            GradeTitle = user.Grade?.Title ?? string.Empty,
            MobileNumber = user.MobileNumber,
            AadharNumber = user.AadharNumber,
            UserType = user.UserType.ToString(),
            EmailVerified = user.EmailVerified,
            ContractorAgencyName = contractorAgencyName
        };
        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existing is null || !existing.IsActive)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        if (!string.IsNullOrWhiteSpace(request.ClientId) && existing.ApplicationClient?.ClientId != request.ClientId)
        {
            throw new InvalidOperationException("Invalid client.");
        }

        var user = existing.User ?? await _dbContext.Users.FirstOrDefaultAsync(x => x.EmployeeId == existing.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.RevokedByIp = ipAddress;

        var newToken = await CreateRefreshTokenAsync(user, ipAddress, existing.ApplicationClient, cancellationToken);
        existing.ReplacedByToken = newToken.Token;

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var response = _tokenService.CreateTokens(user, user.Grade);
        response.RefreshToken = newToken.Token;
        return response;
    }

    public async Task<bool> LogoutAsync(LogoutRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.RevokedByIp = ipAddress;
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SendPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null || !user.EmailVerified)
        {
            return false;
        }

        var token = _tokenService.GenerateSecureToken();
        await ReplaceOneTimeTokenAsync(user.EmployeeId, TokenPurpose.PasswordReset, token, DateTimeOffset.UtcNow.AddMinutes(30), cancellationToken);

        var resetLink =
            $"{ResolvePublicBaseUrl()}/auth/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        var body = $@"
<p>Click the button below to reset your password. This link expires in 30 minutes.</p>
<p><a href=""{resetLink}"">Reset Password</a></p>";

        await _emailService.SendEmailAsync(email, "Reset your password", body, cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var tokenHash = HashToken(request.Token);
        var stored = await _dbContext.OneTimeTokens
            .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId &&
                                      x.Purpose == TokenPurpose.PasswordReset &&
                                      x.TokenHash == tokenHash, cancellationToken);
        if (stored is null || stored.ExpiresAt < DateTimeOffset.UtcNow)
        {
            if (stored is not null)
            {
                _dbContext.OneTimeTokens.Remove(stored);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        _dbContext.OneTimeTokens.Remove(stored);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return false;
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(
        ApplicationUser user,
        string ipAddress,
        ApplicationClient? applicationClient,
        CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.EmployeeId,
            Token = _tokenService.GenerateSecureToken(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            ApplicationClientId = applicationClient?.ApplicationClientId
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    private async Task ReplaceOneTimeTokenAsync(
        string userId,
        TokenPurpose purpose,
        string token,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.OneTimeTokens
            .Where(x => x.UserId == userId && x.Purpose == purpose)
            .ToListAsync(cancellationToken);
        if (existing.Count > 0)
        {
            _dbContext.OneTimeTokens.RemoveRange(existing);
        }

        var entry = new OneTimeToken
        {
            OneTimeTokenId = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(token),
            Purpose = purpose,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.OneTimeTokens.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }


    private string ResolvePublicBaseUrl()
    {
        var configured = _publicBaseUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            return $"{request.Scheme}://{request.Host}".TrimEnd('/');
        }

        return "http://10.0.2.2:5023";
    }
}











