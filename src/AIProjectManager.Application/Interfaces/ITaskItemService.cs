using AIProjectManager.Application.DTOs;

namespace AIProjectManager.Application.Interfaces;

public interface ITaskItemService
{
    Task<TaskItemDto> CreateAsync(CreateTaskItemDto dto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<TaskItemDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItemDto>> GetByProjectIdAsync(Guid projectId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskItemDto dto, Guid tenantId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}

