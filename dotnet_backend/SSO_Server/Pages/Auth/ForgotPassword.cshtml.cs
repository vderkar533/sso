using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class ForgotPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ForgotPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            Error = "Email is required.";
            return Page();
        }

        var sent = await _authService.SendPasswordResetAsync(Email, HttpContext.RequestAborted);
        Message = sent ? "Password reset email sent." : "Invalid email.";
        return Page();
    }
}
