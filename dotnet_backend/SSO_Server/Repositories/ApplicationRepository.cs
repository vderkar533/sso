using Microsoft.EntityFrameworkCore;
using SSO.Server.Data;
using SSO.Server.Models;

namespace SSO.Server.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _dbContext;

    public ApplicationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApplicationClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        return _dbContext.Applications.FirstOrDefaultAsync(x => x.ClientId == clientId && x.IsActive, cancellationToken);
    }
}
