using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IRepository<Project> projectRepository,
        IRepository<User> userRepository,
        IMapper mapper,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Verify user exists and belongs to tenant
        var user = await _userRepository.GetByIdAsync(userId, tenantId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = "Active",
            OwnerId = userId,
            TenantId = tenantId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedBy = userId
        };

        project = await _projectRepository.AddAsync(project, cancellationToken);
        
        // Reload with navigation properties for mapping
        project = await _projectRepository.GetByIdAsync(project.Id, tenantId, cancellationToken);
        if (project == null)
        {
            throw new InvalidOperationException("Failed to retrieve created project");
        }

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId, cancellationToken);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(tenantId, cancellationToken);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Status = dto.Status;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);
        
        // Reload with navigation properties for mapping
        project = await _projectRepository.GetByIdAsync(project.Id, tenantId, cancellationToken);
        if (project == null)
        {
            throw new InvalidOperationException("Failed to retrieve updated project");
        }
        
        return _mapper.Map<ProjectDto>(project);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        await _projectRepository.DeleteAsync(project, cancellationToken);
    }
}

