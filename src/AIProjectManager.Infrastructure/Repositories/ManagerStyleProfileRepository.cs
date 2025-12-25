using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Repositories;

public class ManagerStyleProfileRepository : Repository<ManagerStyleProfile>, IRepository<ManagerStyleProfile>
{
    public ManagerStyleProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ManagerStyleProfile?> GetByUserIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId, cancellationToken);
    }
}

