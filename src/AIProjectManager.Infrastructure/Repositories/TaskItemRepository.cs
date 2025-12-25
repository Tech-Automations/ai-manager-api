using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Repositories;

public class TaskItemRepository : Repository<TaskItem>, IRepository<TaskItem>
{
    public TaskItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<TaskItem?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId, cancellationToken);
    }

    public override async Task<IEnumerable<TaskItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<TaskItem>> FindAsync(System.Linq.Expressions.Expression<Func<TaskItem, bool>> predicate, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(e => e.TenantId == tenantId)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }
}

