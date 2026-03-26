using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Server.DTOs;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ResetPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty(SupportsGet = true)]
    public string Email { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Error = "All fields are required.";
            return Page();
        }

        var success = await _authService.ResetPasswordAsync(
            new ResetPasswordRequest { Email = Email, Token = Token, NewPassword = NewPassword },
            HttpContext.RequestAborted);

        Message = success ? "Password reset successful." : "Invalid or expired token.";
        return Page();
    }
}
