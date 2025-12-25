using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskItemService _taskService;
    private readonly IValidator<CreateTaskItemDto> _createValidator;
    private readonly IValidator<UpdateTaskItemDto> _updateValidator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskItemService taskService,
        IValidator<CreateTaskItemDto> createValidator,
        IValidator<UpdateTaskItemDto> updateValidator,
        ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll([FromQuery] Guid? projectId, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            IEnumerable<TaskItemDto> tasks;
            if (projectId.HasValue)
            {
                tasks = await _taskService.GetByProjectIdAsync(projectId.Value, tenantId, cancellationToken);
            }
            else
            {
                tasks = await _taskService.GetAllAsync(tenantId, cancellationToken);
            }

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var task = await _taskService.GetByIdAsync(id, tenantId, cancellationToken);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskItemDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var task = await _taskService.CreateAsync(dto, userId, tenantId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItemDto>> Update(Guid id, [FromBody] UpdateTaskItemDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var task = await _taskService.UpdateAsync(id, dto, tenantId, cancellationToken);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            await _taskService.DeleteAsync(id, tenantId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}

