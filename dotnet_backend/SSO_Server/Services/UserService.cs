using Microsoft.EntityFrameworkCore;
using SSO.Server.Data;
using SSO.Server.DTOs;
using SSO.Server.Models;

namespace SSO.Server.Services;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileResponse?> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.Grade)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.EmployeeId == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        ContractorProfile? contractorProfile = null;
        if (user.UserType == UserType.Contractor)
        {
            contractorProfile = await _dbContext.ContractorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
        }

        return BuildProfileResponse(user, contractorProfile);
    }

    public async Task<UserProfileResponse?> GetProfileByEmployeeIdAsync(
        string employeeId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return null;
        }

        var user = await _dbContext.Users
            .Include(x => x.Grade)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        ContractorProfile? contractorProfile = null;
        if (user.UserType == UserType.Contractor)
        {
            contractorProfile = await _dbContext.ContractorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
        }

        return BuildProfileResponse(user, contractorProfile);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserType == UserType.Contractor && string.IsNullOrWhiteSpace(request.ContractorAgencyName))
        {
            throw new InvalidOperationException("Contractor agency name is required.");
        }

        if (request.DepartmentId <= 0)
        {
            throw new InvalidOperationException("Please select a department.");
        }

        if (request.GradeId <= 0)
        {
            throw new InvalidOperationException("Please select a grade.");
        }

        var user = await _dbContext.Users
            .Include(x => x.Grade)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.EmployeeId == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Name = request.Name;
        user.DepartmentId = request.DepartmentId;
        user.GradeId = request.GradeId;
        user.MobileNumber = request.MobileNumber;
        user.AadharNumber = request.AadharNumber;
        user.UserType = request.UserType;

        if (request.UserType == UserType.Employee)
        {
            var profile = await _dbContext.EmployeeProfiles
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
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

            var contractor = await _dbContext.ContractorProfiles
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
            if (contractor is not null)
            {
                _dbContext.ContractorProfiles.Remove(contractor);
            }
        }
        else
        {
            var profile = await _dbContext.ContractorProfiles
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
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

            var employee = await _dbContext.EmployeeProfiles
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
            if (employee is not null)
            {
                _dbContext.EmployeeProfiles.Remove(employee);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        user = await _dbContext.Users
            .Include(x => x.Grade)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.EmployeeId == user.EmployeeId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        ContractorProfile? contractorProfile = null;
        if (user.UserType == UserType.Contractor)
        {
            contractorProfile = await _dbContext.ContractorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.EmployeeId, cancellationToken);
        }

        return BuildProfileResponse(user, contractorProfile);
    }

    private static UserProfileResponse BuildProfileResponse(ApplicationUser user, ContractorProfile? contractorProfile)
    {
        return new UserProfileResponse
        {
            UserId = user.EmployeeId,
            EmployeeId = user.EmployeeId,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            DepartmentId = user.DepartmentId,
            Department = user.Department?.DepartmentName ?? string.Empty,
            GradeId = user.GradeId,
            Grade = user.Grade?.GradeCode ?? string.Empty,
            GradeTitle = user.Grade?.Title ?? string.Empty,
            HierarchyLevel = user.Grade?.HierarchyLevel ?? 0,
            UserType = user.UserType.ToString(),
            UserTypeId = (int)user.UserType,
            MobileNumber = user.MobileNumber,
            AadharNumber = user.AadharNumber,
            ContractorAgencyName = contractorProfile?.AgencyName ?? string.Empty
        };
    }
}
