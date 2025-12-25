using AIProjectManager.Application.DTOs;

namespace AIProjectManager.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto, Guid tenantId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}

