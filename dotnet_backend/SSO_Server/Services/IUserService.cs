using SSO.Server.DTOs;

namespace SSO.Server.Services;

public interface IUserService
{
    Task<UserProfileResponse?> GetProfileAsync(string userId, CancellationToken cancellationToken);
    Task<UserProfileResponse?> GetProfileByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken);
    Task<UserProfileResponse?> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken);
}
