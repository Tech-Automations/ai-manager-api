using System.Linq.Expressions;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, IRepository<Tenant>
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Tenant?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // For Tenant, we don't filter by TenantId since Tenant is the top-level entity
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Tenant>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // For Tenant, we return all tenants (this might be restricted in a real scenario)
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // For Tenant, we don't filter by TenantId since Tenant is the top-level entity
        return await _dbSet
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }
}

