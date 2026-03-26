using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SSO.Server.Data;
using SSO.Server.DTOs;
using SSO.Server.Services;

namespace SSO.Server.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _dbContext;

    public AuthController(IAuthService authService, AppDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [HttpPost("send-email-verification")]
    public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sent = await _authService.SendEmailVerificationAsync(request.EmployeeId, request.Email, cancellationToken);
        return sent ? Ok(new { message = "Verification email sent." }) : BadRequest(new { message = "Email already verified or invalid." });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] VerifyEmailRequest request, [FromQuery] string? returnUrl, CancellationToken cancellationToken)
    {
        var verified = await _authService.VerifyEmailAsync(request.Email, request.Token, cancellationToken);
        if (!verified)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return Redirect(returnUrl);
            }

            var safeUrl = WebUtility.HtmlEncode(returnUrl);
            var html = $@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Email verified</title>
  <style>
    body {{ font-family: Arial, sans-serif; background: #f8f9fb; color: #1f2a37; padding: 24px; }}
    .card {{ max-width: 520px; margin: 40px auto; background: white; padding: 24px; border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,0.08); }}
    a.button {{ display: inline-block; margin-top: 16px; padding: 10px 16px; background: #c62828; color: #fff; text-decoration: none; border-radius: 6px; }}
    .muted {{ color: #6b7280; font-size: 14px; }}
  </style>
</head>
<body>
  <div class=""card"">
    <h2>Email verified</h2>
    <p>Your email is verified. Return to the app to continue registration.</p>
    <a class=""button"" href=""{safeUrl}"">Open app</a>
    <p class=""muted"">If the app does not open, you can go back to it and press ""I already verified"".</p>
  </div>
</body>
</html>";
            return Content(html, "text/html");
        }

        return Ok(new { message = "Email verified. Continue registration." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request, GetClientIp(), cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sent = await _authService.SendPasswordResetAsync(request.Email, cancellationToken);
        return sent ? Ok(new { message = "Password reset email sent." }) : BadRequest(new { message = "Invalid email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var reset = await _authService.ResetPasswordAsync(request, cancellationToken);
        return reset ? Ok(new { message = "Password reset successful." }) : BadRequest(new { message = "Invalid or expired token." });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var changed = await _authService.ChangePasswordAsync(request, cancellationToken);
        return changed ? Ok(new { message = "Password changed successfully." }) : BadRequest(new { message = "Current password is incorrect." });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RefreshTokenAsync(request, GetClientIp(), cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LogoutAsync(request, GetClientIp(), cancellationToken);
        return result ? Ok(new { message = "Logged out." }) : BadRequest(new { message = "Invalid refresh token." });
    }

    // Temporary debug endpoint to verify registration state.
    [HttpGet("debug-user")]
    public async Task<IActionResult> DebugUser([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new
        {
            user.Email,
            user.EmailVerified,
            HasPassword = !string.IsNullOrWhiteSpace(user.PasswordHash)
        });
    }

    private string GetClientIp()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            return forwarded.ToString().Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}



