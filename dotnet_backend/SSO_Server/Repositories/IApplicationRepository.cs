using SSO.Server.Models;

namespace SSO.Server.Repositories;

public interface IApplicationRepository
{
    Task<ApplicationClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken);
}
