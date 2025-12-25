using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Repositories;

public class ChatSessionRepository : Repository<ChatSession>, IRepository<ChatSession>
{
    public ChatSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(Guid userId, Guid tenantId, Guid? projectId = null, DateTime? dateFrom = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(c => c.UserId == userId && c.TenantId == tenantId)
            .Where(c => c.ParentSessionId == null) // Only top-level sessions
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(c => c.ProjectId == projectId);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= dateFrom.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .Include(c => c.Project)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession?> GetByIdWithIncludesAsync(Guid id, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Project)
            .Include(c => c.FollowUps)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<ChatSession?> GetByIdWithUserAsync(Guid id, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<IEnumerable<ChatSession>> GetRecentSessionsAsync(Guid userId, Guid tenantId, int count = 3, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && c.TenantId == tenantId && c.ParentSessionId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession?> GetParentSessionAsync(Guid parentSessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Id == parentSessionId && c.UserId == userId, cancellationToken);
    }
}

