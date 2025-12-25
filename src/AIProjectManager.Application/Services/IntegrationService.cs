using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIProjectManager.Application.DTOs.Integration;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class IntegrationService : IIntegrationService
{
    private readonly IRepository<Integration> _integrationRepository;
    private readonly IRepository<ExternalWorkItem> _workItemRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ILogger<IntegrationService> _logger;

    public IntegrationService(
        IRepository<Integration> integrationRepository,
        IRepository<ExternalWorkItem> workItemRepository,
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<IntegrationService> logger)
    {
        _integrationRepository = integrationRepository;
        _workItemRepository = workItemRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IntegrationDto> ConnectAzureDevOpsAsync(ConnectAzureDevOpsDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Validate connection by making a test API call
        var baseUrl = _configuration["AzureDevOps:BaseUrl"] ?? "https://dev.azure.com";
        var apiVersion = _configuration["AzureDevOps:ApiVersion"] ?? "7.1";
        var orgUrl = request.OrganizationUrl.TrimEnd('/');
        
        // Test connection
        var testUrl = $"{orgUrl}/_apis/projects?api-version={apiVersion}";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{request.PersonalAccessToken}")));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await _httpClient.GetAsync(testUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to connect to Azure DevOps: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Azure DevOps connection");
            throw new InvalidOperationException("Failed to connect to Azure DevOps. Please check your organization URL and Personal Access Token.", ex);
        }

        // Store configuration (in production, encrypt the PAT)
        var config = new
        {
            OrganizationUrl = orgUrl,
            PersonalAccessToken = request.PersonalAccessToken, // TODO: Encrypt this
            ProjectName = request.ProjectName
        };

        // Check if integration already exists
        var existingIntegrations = await _integrationRepository.GetAllAsync(tenantId, cancellationToken);
        var existing = existingIntegrations.FirstOrDefault(i => i.Type == "AzureDevOps" && i.Name == orgUrl);

        Integration integration;
        if (existing != null)
        {
            existing.Configuration = JsonSerializer.Serialize(config);
            existing.IsActive = true;
            existing.Status = "Connected";
            existing.ErrorMessage = null;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _integrationRepository.UpdateAsync(existing, cancellationToken);
            integration = existing;
        }
        else
        {
            integration = new Integration
            {
                Type = "AzureDevOps",
                Name = orgUrl,
                Configuration = JsonSerializer.Serialize(config),
                IsActive = true,
                Status = "Connected",
                TenantId = tenantId,
                CreatedBy = userId
            };
            integration = await _integrationRepository.AddAsync(integration, cancellationToken);
        }

        return _mapper.Map<IntegrationDto>(integration);
    }

    public async Task<IntegrationDto> GetIntegrationAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId, tenantId, cancellationToken);
        if (integration == null)
        {
            throw new KeyNotFoundException("Integration not found");
        }

        return _mapper.Map<IntegrationDto>(integration);
    }

    public async Task<IEnumerable<IntegrationDto>> GetIntegrationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var integrations = await _integrationRepository.GetAllAsync(tenantId, cancellationToken);
        return _mapper.Map<IEnumerable<IntegrationDto>>(integrations);
    }

    public async Task DisconnectIntegrationAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId, tenantId, cancellationToken);
        if (integration == null)
        {
            throw new KeyNotFoundException("Integration not found");
        }

        integration.IsActive = false;
        integration.Status = "Disconnected";
        integration.UpdatedAt = DateTime.UtcNow;
        
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
    }

    public async Task<IntegrationDto> SyncAzureDevOpsAsync(Guid integrationId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId, tenantId, cancellationToken);
        if (integration == null || integration.Type != "AzureDevOps")
        {
            throw new KeyNotFoundException("Azure DevOps integration not found");
        }

        var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(integration.Configuration ?? "{}") ?? new Dictionary<string, JsonElement>();
        var orgUrl = config.GetValueOrDefault("OrganizationUrl").GetString() ?? throw new InvalidOperationException("Invalid integration configuration");
        var pat = config.GetValueOrDefault("PersonalAccessToken").GetString() ?? throw new InvalidOperationException("Invalid integration configuration");
        var apiVersion = _configuration["AzureDevOps:ApiVersion"] ?? "7.1";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            // Get work items (simplified - in production, use Work Items API)
            var projectsUrl = $"{orgUrl}/_apis/projects?api-version={apiVersion}";
            var projectsResponse = await _httpClient.GetStringAsync(projectsUrl, cancellationToken);
            
            // TODO: Implement proper work item sync using Work Items API
            // For now, just update last sync time
            
            integration.LastSyncedAt = DateTime.UtcNow;
            integration.Status = "Connected";
            integration.ErrorMessage = null;
            await _integrationRepository.UpdateAsync(integration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Azure DevOps integration");
            integration.Status = "Error";
            integration.ErrorMessage = ex.Message;
            await _integrationRepository.UpdateAsync(integration, cancellationToken);
            throw;
        }

        return _mapper.Map<IntegrationDto>(integration);
    }

    public async Task<IEnumerable<ExternalWorkItemDto>> GetWorkItemsAsync(Guid integrationId, Guid tenantId, Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        var workItems = await _workItemRepository.GetAllAsync(tenantId, cancellationToken);
        var filtered = workItems.Where(w => w.IntegrationId == integrationId);

        if (projectId.HasValue)
        {
            // Filter by project through linked tasks
            var tasks = await _taskRepository.GetAllAsync(tenantId, cancellationToken);
            var projectTaskIds = tasks.Where(t => t.ProjectId == projectId.Value).Select(t => t.Id).ToHashSet();
            filtered = filtered.Where(w => w.TaskItemId.HasValue && projectTaskIds.Contains(w.TaskItemId.Value));
        }

        return _mapper.Map<IEnumerable<ExternalWorkItemDto>>(filtered);
    }

    public async Task<ExternalWorkItemDto> LinkWorkItemToTaskAsync(Guid workItemId, Guid taskItemId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId, tenantId, cancellationToken);
        if (workItem == null)
        {
            throw new KeyNotFoundException("Work item not found");
        }

        var task = await _taskRepository.GetByIdAsync(taskItemId, tenantId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        workItem.TaskItemId = taskItemId;
        workItem.UpdatedAt = DateTime.UtcNow;
        await _workItemRepository.UpdateAsync(workItem, cancellationToken);

        return _mapper.Map<ExternalWorkItemDto>(workItem);
    }
}

