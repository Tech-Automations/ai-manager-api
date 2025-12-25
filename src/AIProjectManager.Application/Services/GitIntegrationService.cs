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

public class GitIntegrationService : IGitIntegrationService
{
    private readonly IRepository<GitRepository> _repositoryRepository;
    private readonly IRepository<GitCommit> _commitRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GitIntegrationService> _logger;

    public GitIntegrationService(
        IRepository<GitRepository> repositoryRepository,
        IRepository<GitCommit> commitRepository,
        IRepository<Project> projectRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<GitIntegrationService> logger)
    {
        _repositoryRepository = repositoryRepository;
        _commitRepository = commitRepository;
        _projectRepository = projectRepository;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GitRepositoryDto> ConnectRepositoryAsync(ConnectGitRepositoryDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Validate project if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, tenantId, cancellationToken);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }
        }

        // Validate repository access (test API call based on provider)
        try
        {
            await ValidateRepositoryAccessAsync(request.Provider, request.RepositoryUrl, request.AccessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Git repository access");
            throw new InvalidOperationException($"Failed to connect to {request.Provider} repository. Please check your repository URL and access token.", ex);
        }

        // Check if repository already connected
        var existingRepos = await _repositoryRepository.GetAllAsync(tenantId, cancellationToken);
        var existing = existingRepos.FirstOrDefault(r => r.RepositoryUrl == request.RepositoryUrl && r.Provider == request.Provider);

        GitRepository repository;
        if (existing != null)
        {
            existing.AccessToken = request.AccessToken; // TODO: Encrypt this
            existing.IsActive = true;
            existing.ProjectId = request.ProjectId;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repositoryRepository.UpdateAsync(existing, cancellationToken);
            repository = existing;
        }
        else
        {
            repository = new GitRepository
            {
                RepositoryUrl = request.RepositoryUrl,
                Provider = request.Provider,
                AccessToken = request.AccessToken, // TODO: Encrypt this
                ProjectId = request.ProjectId,
                IsActive = true,
                TenantId = tenantId,
                CreatedBy = userId
            };
            repository = await _repositoryRepository.AddAsync(repository, cancellationToken);
        }

        return _mapper.Map<GitRepositoryDto>(repository);
    }

    public async Task<GitRepositoryDto> GetRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, tenantId, cancellationToken);
        if (repository == null)
        {
            throw new KeyNotFoundException("Repository not found");
        }

        return _mapper.Map<GitRepositoryDto>(repository);
    }

    public async Task<IEnumerable<GitRepositoryDto>> GetRepositoriesAsync(Guid tenantId, Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        var repositories = await _repositoryRepository.GetAllAsync(tenantId, cancellationToken);
        var filtered = repositories.AsQueryable();

        if (projectId.HasValue)
        {
            filtered = filtered.Where(r => r.ProjectId == projectId);
        }

        return _mapper.Map<IEnumerable<GitRepositoryDto>>(filtered.ToList());
    }

    public async Task DisconnectRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, tenantId, cancellationToken);
        if (repository == null)
        {
            throw new KeyNotFoundException("Repository not found");
        }

        repository.IsActive = false;
        repository.UpdatedAt = DateTime.UtcNow;
        
        await _repositoryRepository.UpdateAsync(repository, cancellationToken);
    }

    public async Task<GitRepositoryDto> SyncRepositoryAsync(Guid repositoryId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, tenantId, cancellationToken);
        if (repository == null)
        {
            throw new KeyNotFoundException("Repository not found");
        }

        try
        {
            // TODO: Implement actual commit fetching from Git provider API
            // For now, just update last sync time
            repository.LastSyncedAt = DateTime.UtcNow;
            await _repositoryRepository.UpdateAsync(repository, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Git repository");
            throw;
        }

        return _mapper.Map<GitRepositoryDto>(repository);
    }

    public async Task<IEnumerable<GitCommitDto>> GetCommitsAsync(Guid repositoryId, Guid tenantId, Guid? projectId = null, DateTime? dateFrom = null, CancellationToken cancellationToken = default)
    {
        var commits = await _commitRepository.GetAllAsync(tenantId, cancellationToken);
        var filtered = commits.Where(c => c.RepositoryId == repositoryId);

        if (projectId.HasValue)
        {
            filtered = filtered.Where(c => c.ProjectId == projectId);
        }

        if (dateFrom.HasValue)
        {
            filtered = filtered.Where(c => c.CommittedAt >= dateFrom.Value);
        }

        return _mapper.Map<IEnumerable<GitCommitDto>>(filtered.OrderByDescending(c => c.CommittedAt));
    }

    public async Task HandleWebhookAsync(string provider, string payload, CancellationToken cancellationToken = default)
    {
        // TODO: Implement webhook handling for Git providers
        // This would parse webhook payloads and create/update commits
        _logger.LogInformation($"Received webhook from {provider}");
    }

    private async Task ValidateRepositoryAccessAsync(string provider, string repositoryUrl, string accessToken, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AIProjectManager/1.0");

        switch (provider.ToLower())
        {
            case "github":
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                // Extract owner/repo from URL and test API access
                var githubUrl = ExtractGitHubApiUrl(repositoryUrl);
                var response = await _httpClient.GetAsync(githubUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Failed to access GitHub repository: {response.StatusCode}");
                }
                break;

            case "gitlab":
                _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", accessToken);
                // Similar validation for GitLab
                break;

            case "bitbucket":
                // Similar validation for Bitbucket
                break;

            default:
                throw new NotSupportedException($"Git provider {provider} is not supported");
        }
    }

    private string ExtractGitHubApiUrl(string repositoryUrl)
    {
        // Convert https://github.com/owner/repo to https://api.github.com/repos/owner/repo
        var uri = new Uri(repositoryUrl);
        var parts = uri.AbsolutePath.Trim('/').Split('/');
        if (parts.Length >= 2)
        {
            return $"https://api.github.com/repos/{parts[0]}/{parts[1]}";
        }
        throw new ArgumentException("Invalid GitHub repository URL");
    }
}

