using SSO.Server.Models;

namespace SSO.Server.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
