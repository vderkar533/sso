using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSO.Server.DTOs;
using SSO.Server.Services;

namespace SSO.Server.Controllers;

[ApiController]
[Route("api/user")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.GetProfileAsync(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("employee/{employeeId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEmployeeId([FromRoute] string employeeId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return BadRequest(new { message = "EmployeeId is required." });
        }

        var profile = await _userService.GetProfileByEmployeeIdAsync(employeeId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
            return profile is null ? NotFound() : Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetUserId()
    {
        var claim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null)
        {
            return null;
        }

        return claim.Value;
    }
}
