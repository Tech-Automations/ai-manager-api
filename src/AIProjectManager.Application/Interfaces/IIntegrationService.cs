using AIProjectManager.Application.DTOs.Integration;

namespace AIProjectManager.Application.Interfaces;

public interface IIntegrationService
{
    Task<IntegrationDto> ConnectAzureDevOpsAsync(ConnectAzureDevOpsDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IntegrationDto> GetIntegrationAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationDto>> GetIntegrationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task DisconnectIntegrationAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IntegrationDto> SyncAzureDevOpsAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExternalWorkItemDto>> GetWorkItemsAsync(Guid integrationId, Guid tenantId, Guid? projectId = null, CancellationToken cancellationToken = default);
    Task<ExternalWorkItemDto> LinkWorkItemToTaskAsync(Guid workItemId, Guid taskItemId, Guid tenantId, CancellationToken cancellationToken = default);
}

