using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Server.DTOs;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class LogoutModel : PageModel
{
    private readonly IAuthService _authService;

    public LogoutModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string RefreshToken { get; set; } = string.Empty;

    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(RefreshToken))
        {
            Error = "Refresh token is required.";
            return Page();
        }

        var result = await _authService.LogoutAsync(
            new LogoutRequest { RefreshToken = RefreshToken },
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "ui",
            HttpContext.RequestAborted);

        if (!result)
        {
            Error = "Invalid refresh token.";
            return Page();
        }

        Message = "Logged out. Refresh token revoked.";
        return Page();
    }
}
