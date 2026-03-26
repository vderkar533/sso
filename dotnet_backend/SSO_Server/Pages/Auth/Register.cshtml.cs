using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SSO.Server.Data;
using SSO.Server.DTOs;
using SSO.Server.Models;
using SSO.Server.Services;

namespace SSO.Server.Pages.Auth;

public sealed class RegisterModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;

    public RegisterModel(AppDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    [BindProperty]
    public RegisterRequest Input { get; set; } = new();

    [BindProperty]
    public VerificationInput Verification { get; set; } = new();

    public List<SelectListItem> GradeOptions { get; set; } = new();
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public string? Message { get; set; }
    public string? Error { get; set; }
    public bool ShowRegisterValidation { get; set; }
    public bool RequirePassword { get; set; }
    public bool RegistrationCompleted { get; set; }

    public async Task OnGetAsync(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Input.Email = email;
            Verification.Email = email;

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (user is not null)
            {
                Input.EmployeeId = user.EmployeeId;
                Input.Name = user.Name;
                Input.MobileNumber = user.MobileNumber;
                Input.AadharNumber = user.AadharNumber;
                Input.UserType = user.UserType;

                if (user.DepartmentId.HasValue)
                {
                    Input.DepartmentId = user.DepartmentId.Value;
                }

                if (user.GradeId.HasValue)
                {
                    Input.GradeId = user.GradeId.Value;
                }

                RequirePassword = user.EmailVerified;

                if (user.UserType == UserType.Contractor)
                {
                    var contractor = await _dbContext.ContractorProfiles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId);
                    if (contractor is not null)
                    {
                        Input.ContractorAgencyName = contractor.AgencyName;
                    }
                }
            }
        }

        await LoadGradesAsync();
        await LoadDepartmentsAsync();
    }

    public async Task<IActionResult> OnPostSendVerificationAsync()
    {
        await LoadGradesAsync();
        await LoadDepartmentsAsync();

        ShowRegisterValidation = false;
        Error = null;
        Message = null;

        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(Verification.Email))
        {
            var rawEmail = Request.Form["Verification.Email"].ToString();
            if (string.IsNullOrWhiteSpace(rawEmail))
            {
                rawEmail = Request.Form["Input.Email"].ToString();
            }

            if (!string.IsNullOrWhiteSpace(rawEmail))
            {
                Verification.Email = rawEmail;
            }
        }

        if (string.IsNullOrWhiteSpace(Verification.EmployeeId))
        {
            var rawEmployeeId = Request.Form["Verification.EmployeeId"].ToString();
            if (string.IsNullOrWhiteSpace(rawEmployeeId))
            {
                rawEmployeeId = Request.Form["Input.EmployeeId"].ToString();
            }

            if (!string.IsNullOrWhiteSpace(rawEmployeeId))
            {
                Verification.EmployeeId = rawEmployeeId;
            }
        }

        if (!TryValidateModel(Verification, nameof(Verification)))
        {
            Error = "Email is required.";
            return Page();
        }

        var sent = await _authService.SendEmailVerificationAsync(Verification.EmployeeId, Verification.Email, HttpContext.RequestAborted);
        Message = sent ? "Verification email sent." : "Email already verified or invalid.";
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        await LoadGradesAsync();
        await LoadDepartmentsAsync();

        Verification.Email = Input.Email;
        ModelState.Remove(nameof(Verification));
        var nonInputKeys = ModelState.Keys
            .Where(k => !k.StartsWith("Input.", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var key in nonInputKeys)
        {
            ModelState.Remove(key);
        }
        ShowRegisterValidation = true;

        var existingUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == Input.Email);
        var emailVerified = existingUser?.EmailVerified == true;
        RequirePassword = emailVerified;
        if (!emailVerified)
        {
            ModelState.Remove("Input.Password");
        }

        if (Input.UserType == UserType.Contractor && string.IsNullOrWhiteSpace(Input.ContractorAgencyName))
        {
            ModelState.AddModelError("Input.ContractorAgencyName", "Contractor agency name is required.");
        }
        if (Input.DepartmentId <= 0)
        {
            ModelState.AddModelError("Input.DepartmentId", "Please select a department.");
        }
        if (Input.GradeId <= 0)
        {
            ModelState.AddModelError("Input.GradeId", "Please select a grade.");
        }
        if (Input.UserType != UserType.Employee && Input.UserType != UserType.Contractor)
        {
            ModelState.AddModelError("Input.UserType", "Please select a user type.");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            Error = errors.Count > 0
                ? string.Join(" | ", errors!)
                : "Please fill all required fields.";
            return Page();
        }

        try
        {
            var response = await _authService.RegisterAsync(Input, HttpContext.RequestAborted);
            var tokenPreview = response.AccessToken.Length > 20
                ? response.AccessToken[..20]
                : response.AccessToken;
            Message = "Registration successful.";
            RegistrationCompleted = true;
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            if (string.Equals(ex.Message, "Email must be verified before registration.", StringComparison.OrdinalIgnoreCase))
            {
                var sent = await _authService.SendEmailVerificationAsync(Input.EmployeeId, Input.Email, HttpContext.RequestAborted);
                Message = sent
                    ? "Verification email sent. Please verify your email before completing registration."
                    : "Email already verified or invalid.";
                return Page();
            }

            Error = ex.Message;
        }

        return Page();
    }

    public sealed class VerificationInput
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required, EmailAddress]
        [RegularExpression(@"(?i)^[A-Z0-9._%+-]+@lloyds\.in$", ErrorMessage = "Email must be a @lloyds.in address.")]
        public string Email { get; set; } = string.Empty;
    }

    private async Task LoadGradesAsync()
    {
        var grades = await _dbContext.Grades.AsNoTracking().ToListAsync();
        GradeOptions = grades
            .Select(g => new SelectListItem($"{g.GradeCode} - {g.Title}", g.GradeId.ToString()))
            .ToList();

        if (GradeOptions.Count == 0)
        {
            Error = "Grades not loaded. Run migrations and seed data.";
        }
    }

    private async Task LoadDepartmentsAsync()
    {
        var departments = await _dbContext.Departments.AsNoTracking().ToListAsync();
        DepartmentOptions = departments
            .Select(d => new SelectListItem(d.DepartmentName, d.DepartmentId.ToString()))
            .ToList();

        if (DepartmentOptions.Count == 0)
        {
            Error = "Departments not loaded. Run migrations and seed data.";
        }
    }
}






