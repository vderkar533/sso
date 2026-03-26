using Microsoft.EntityFrameworkCore;
using SSO.Server.Data;
using SSO.Server.Models;

namespace SSO.Server.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x!.Grade)
            .Include(x => x.ApplicationClient)
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
