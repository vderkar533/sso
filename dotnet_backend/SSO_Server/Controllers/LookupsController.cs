using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Server.Data;
using SSO.Server.DTOs;

namespace SSO.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/lookups")]
public sealed class LookupsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public LookupsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("departments")]
    public async Task<ActionResult<List<DepartmentOption>>> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentOption
            {
                Id = d.DepartmentId,
                Name = d.DepartmentName
            })
            .ToListAsync(cancellationToken);

        return Ok(departments);
    }

    [HttpGet("grades")]
    public async Task<ActionResult<List<GradeOption>>> GetGrades(CancellationToken cancellationToken)
    {
        var grades = await _dbContext.Grades
            .AsNoTracking()
            .OrderBy(g => g.GradeId)
            .Select(g => new GradeOption
            {
                Id = g.GradeId,
                Code = g.GradeCode,
                Title = g.Title,
                HierarchyLevel = g.HierarchyLevel
            })
            .ToListAsync(cancellationToken);

        return Ok(grades);
    }
}
