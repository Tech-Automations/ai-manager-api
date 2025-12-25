using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IRepository<Project>
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Project?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId, cancellationToken);
    }

    public override async Task<IEnumerable<Project>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Owner)
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}

