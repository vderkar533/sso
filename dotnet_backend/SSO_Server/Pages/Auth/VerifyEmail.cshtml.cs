using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class VerifyEmailModel : PageModel
{
    private readonly IAuthService _authService;

    public VerifyEmailModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty(SupportsGet = true)]
    public string Email { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
        {
            return;
        }

        var verified = await _authService.VerifyEmailAsync(Email, Token, HttpContext.RequestAborted);
        if (verified)
        {
            Message = "Email verified. Continue registration.";
        }
        else
        {
            Error = "Invalid or expired token.";
        }
    }
}
