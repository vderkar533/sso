using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using SSO.Server.Data;
using SSO.Server.DTOs;
using SSO.Server.Models;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _dbContext;

    public LoginModel(IAuthService authService, AppDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Password { get; set; } = string.Empty;

    [BindProperty, ValidateNever]
    public EditProfileInput Edit { get; set; } = new();

    [BindProperty, ValidateNever]
    public string RefreshToken { get; set; } = string.Empty;

    [BindProperty, ValidateNever]
    public ChangePasswordInput Reset { get; set; } = new();

    [BindProperty]
    public string? ActiveTab { get; set; } = "edit";

    public List<SelectListItem> GradeOptions { get; set; } = new();
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public string? Message { get; set; }
    public string? Error { get; set; }
    public bool ShowEditForm { get; set; }

    public async Task OnGetAsync()
    {
        ActiveTab = "edit";
        Error = null;
        Message = null;
        await LoadGradesAsync();
        await LoadDepartmentsAsync();
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        ModelState.Remove(nameof(Edit));
        ModelState.Remove(nameof(RefreshToken));
        ModelState.Remove(nameof(Reset));

        var allowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Email),
            nameof(Password),
            nameof(ActiveTab)
        };
        var keysToRemove = ModelState.Keys
            .Where(k => !allowedKeys.Contains(k))
            .ToList();
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            ActiveTab ??= "edit";
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();
            Error = errors.Count > 0
                ? string.Join(" | ", errors)
                : "Invalid input.";
            await LoadGradesAsync();
            await LoadDepartmentsAsync();
            return Page();
        }

        try
        {
            var response = await _authService.LoginAsync(
                new LoginRequest { Email = Email, Password = Password },
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "ui",
                HttpContext.RequestAborted);

            RefreshToken = response.RefreshToken;
            ShowEditForm = true;
            ActiveTab = "edit";

            if (response.User is null)
            {
                Error = "Login succeeded but no user profile was returned.";
                ShowEditForm = false;
                return Page();
            }

            Edit = new EditProfileInput
            {
                UserId = response.User.UserId,
                EmployeeId = response.User.EmployeeId,
                Name = response.User.Name,
                Email = response.User.Email,
                DepartmentId = response.User.DepartmentId ?? 0,
                GradeId = response.User.GradeId ?? 0,
                MobileNumber = response.User.MobileNumber,
                AadharNumber = response.User.AadharNumber,
                UserType = Enum.TryParse<UserType>(response.User.UserType, true, out var userType)
                    ? userType
                    : UserType.Employee,
                ContractorAgencyName = response.User.ContractorAgencyName
            };
            Reset = new ChangePasswordInput { Email = Edit.Email };

            Message = "Login successful. Edit your details, save, then logout.";
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
        }

        await LoadGradesAsync();
        await LoadDepartmentsAsync();
        ActiveTab ??= "edit";
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ShowEditForm = true;
        ActiveTab = "edit";
        await LoadGradesAsync();
        await LoadDepartmentsAsync();

        ModelState.Clear();
        if (!TryValidateModel(Edit, nameof(Edit)))
        {
            Error = "Please fill all required fields.";
            return Page();
        }

        if (Edit.UserType == UserType.Contractor && string.IsNullOrWhiteSpace(Edit.ContractorAgencyName))
        {
            ModelState.AddModelError("Edit.ContractorAgencyName", "Contractor agency name is required.");
            Error = "Contractor agency name is required.";
            return Page();
        }

        if (Edit.DepartmentId <= 0)
        {
            ModelState.AddModelError("Edit.DepartmentId", "Please select a department.");
            Error = "Please select a department.";
            return Page();
        }

        if (Edit.GradeId <= 0)
        {
            ModelState.AddModelError("Edit.GradeId", "Please select a grade.");
            Error = "Please select a grade.";
            return Page();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.EmployeeId == Edit.UserId, HttpContext.RequestAborted);
        if (user is null)
        {
            Error = "User not found.";
            return Page();
        }

        user.Name = Edit.Name;
        user.DepartmentId = Edit.DepartmentId;
        user.GradeId = Edit.GradeId;
        user.MobileNumber = Edit.MobileNumber;
        user.AadharNumber = Edit.AadharNumber;
        user.UserType = Edit.UserType;

        if (Edit.UserType == UserType.Employee)
        {
            var profile = await _dbContext.EmployeeProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, HttpContext.RequestAborted);
            if (profile is null)
            {
                profile = new EmployeeProfile { UserId = user.EmployeeId };
                await _dbContext.EmployeeProfiles.AddAsync(profile, HttpContext.RequestAborted);
            }

            profile.EmployeeId = Edit.EmployeeId;
            profile.Name = Edit.Name;
            profile.DepartmentId = Edit.DepartmentId;
            profile.GradeId = Edit.GradeId;
            profile.MobileNumber = Edit.MobileNumber;
            profile.AadharNumber = Edit.AadharNumber;

            var contractor = await _dbContext.ContractorProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, HttpContext.RequestAborted);
            if (contractor is not null)
            {
                _dbContext.ContractorProfiles.Remove(contractor);
            }
        }
        else
        {
            var profile = await _dbContext.ContractorProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, HttpContext.RequestAborted);
            if (profile is null)
            {
                profile = new ContractorProfile { UserId = user.EmployeeId };
                await _dbContext.ContractorProfiles.AddAsync(profile, HttpContext.RequestAborted);
            }

            profile.EmployeeId = Edit.EmployeeId;
            profile.Name = Edit.Name;
            profile.DepartmentId = Edit.DepartmentId;
            profile.GradeId = Edit.GradeId;
            profile.MobileNumber = Edit.MobileNumber;
            profile.AadharNumber = Edit.AadharNumber;
            profile.AgencyName = Edit.ContractorAgencyName?.Trim() ?? string.Empty;

            var employee = await _dbContext.EmployeeProfiles.FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, HttpContext.RequestAborted);
            if (employee is not null)
            {
                _dbContext.EmployeeProfiles.Remove(employee);
            }
        }

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

        Reset.Email = Edit.Email;
        Message = "Profile updated. You can logout now.";
        return Page();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        ShowEditForm = true;
        ActiveTab = "reset";
        await LoadGradesAsync();
        await LoadDepartmentsAsync();

        ModelState.Clear();
        if (!TryValidateModel(Reset, nameof(Reset)))
        {
            Error = "Please fill all required fields.";
            return Page();
        }

        var success = await _authService.ChangePasswordAsync(
            new ChangePasswordRequest
            {
                Email = Reset.Email,
                CurrentPassword = Reset.CurrentPassword,
                NewPassword = Reset.NewPassword
            },
            HttpContext.RequestAborted);

        Message = success ? "Password changed successfully." : "Current password is incorrect.";
        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        ShowEditForm = false;
        await LoadGradesAsync();
        await LoadDepartmentsAsync();

        if (string.IsNullOrWhiteSpace(RefreshToken))
        {
            Message = "Logged out.";
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

        Message = "Logged out.";
        return Page();
    }

    public sealed class EditProfileInput
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int GradeId { get; set; }

        [Required]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string AadharNumber { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        public string? ContractorAgencyName { get; set; }
    }

    public sealed class ChangePasswordInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    private async Task LoadGradesAsync()
    {
        var grades = await _dbContext.Grades.AsNoTracking().ToListAsync();
        GradeOptions = grades
            .Select(g => new SelectListItem($"{g.GradeCode} - {g.Title}", g.GradeId.ToString()))
            .ToList();
    }

    private async Task LoadDepartmentsAsync()
    {
        var departments = await _dbContext.Departments.AsNoTracking().ToListAsync();
        DepartmentOptions = departments
            .Select(d => new SelectListItem(d.DepartmentName, d.DepartmentId.ToString()))
            .ToList();
    }
}
