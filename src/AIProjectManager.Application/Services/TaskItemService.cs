using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class TaskItemService : ITaskItemService
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskItemService> _logger;

    public TaskItemService(
        IRepository<TaskItem> taskRepository,
        IRepository<Project> projectRepository,
        IRepository<User> userRepository,
        IMapper mapper,
        ILogger<TaskItemService> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TaskItemDto> CreateAsync(CreateTaskItemDto dto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Verify project exists and belongs to tenant
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, tenantId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        // Verify assigned user exists if provided
        User? assignedUser = null;
        if (dto.AssignedToId.HasValue)
        {
            assignedUser = await _userRepository.GetByIdAsync(dto.AssignedToId.Value, tenantId, cancellationToken);
            if (assignedUser == null)
            {
                throw new KeyNotFoundException("Assigned user not found");
            }
        }

        var taskItem = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = "Todo",
            ProjectId = dto.ProjectId,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate,
            TenantId = tenantId,
            CreatedBy = userId
        };

        taskItem = await _taskRepository.AddAsync(taskItem, cancellationToken);
        
        // Reload with navigation properties for mapping
        taskItem = await _taskRepository.GetByIdAsync(taskItem.Id, tenantId, cancellationToken);
        if (taskItem == null)
        {
            throw new InvalidOperationException("Failed to retrieve created task");
        }

        return _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task<TaskItemDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, tenantId, cancellationToken);
        return taskItem == null ? null : _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task<IEnumerable<TaskItemDto>> GetByProjectIdAsync(Guid projectId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.FindAsync(t => t.ProjectId == projectId, tenantId, cancellationToken);
        return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(tenantId, cancellationToken);
        return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
    }

    public async Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskItemDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, tenantId, cancellationToken);
        if (taskItem == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        // Verify assigned user exists if provided
        if (dto.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(dto.AssignedToId.Value, tenantId, cancellationToken);
            if (assignedUser == null)
            {
                throw new KeyNotFoundException("Assigned user not found");
            }
        }

        taskItem.Title = dto.Title;
        taskItem.Description = dto.Description;
        taskItem.Status = dto.Status;
        taskItem.Priority = dto.Priority;
        taskItem.AssignedToId = dto.AssignedToId;
        taskItem.DueDate = dto.DueDate;
        taskItem.UpdatedAt = DateTime.UtcNow;

        // Update CompletedAt if status is Done
        if (dto.Status == "Done" && taskItem.CompletedAt == null)
        {
            taskItem.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.Status != "Done")
        {
            taskItem.CompletedAt = null;
        }

        await _taskRepository.UpdateAsync(taskItem, cancellationToken);
        
        // Reload with navigation properties for mapping
        taskItem = await _taskRepository.GetByIdAsync(taskItem.Id, tenantId, cancellationToken);
        if (taskItem == null)
        {
            throw new InvalidOperationException("Failed to retrieve updated task");
        }
        
        return _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, tenantId, cancellationToken);
        if (taskItem == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        await _taskRepository.DeleteAsync(taskItem, cancellationToken);
    }
}

