using AIProjectManager.Application.DTOs.Integration;

namespace AIProjectManager.Application.Interfaces;

public interface IGitIntegrationService
{
    Task<GitRepositoryDto> ConnectRepositoryAsync(ConnectGitRepositoryDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<GitRepositoryDto> GetRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GitRepositoryDto>> GetRepositoriesAsync(Guid tenantId, Guid? projectId = null, CancellationToken cancellationToken = default);
    Task DisconnectRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<GitRepositoryDto> SyncRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GitCommitDto>> GetCommitsAsync(Guid repositoryId, Guid tenantId, Guid? projectId = null, DateTime? dateFrom = null, CancellationToken cancellationToken = default);
    Task HandleWebhookAsync(string provider, string payload, CancellationToken cancellationToken = default);
}

