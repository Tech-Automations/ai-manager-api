using AIProjectManager.Application.DTOs.Chat;
using AIProjectManager.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProjectManager.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IValidator<ChatQueryDto> _chatQueryValidator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IValidator<ChatQueryDto> chatQueryValidator,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _chatQueryValidator = chatQueryValidator;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<ActionResult<ChatResponseDto>> SendQuery([FromBody] ChatQueryDto query, CancellationToken cancellationToken)
    {
        var validationResult = await _chatQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var response = await _chatService.SendQueryAsync(query, userId, tenantId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat query");
            return StatusCode(500, new { error = "An error occurred while processing your query" });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetHistory(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var history = await _chatService.GetChatHistoryAsync(userId, tenantId, projectId, dateFrom, limit, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("history/{id}")]
    public async Task<ActionResult<ChatSessionDto>> GetSession(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var session = await _chatService.GetChatSessionAsync(id, userId, tenantId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Chat session not found" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat session");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpDelete("history/{id}")]
    public async Task<ActionResult> DeleteSession(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            await _chatService.DeleteChatSessionAsync(id, userId, tenantId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat session");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("style-profile")]
    public async Task<ActionResult<ManagerStyleProfileDto>> GetStyleProfile(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var profile = await _chatService.GetStyleProfileAsync(userId, tenantId, cancellationToken);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving style profile");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPut("style-profile")]
    public async Task<ActionResult<ManagerStyleProfileDto>> UpdateStyleProfile(
        [FromBody] UpdateManagerStyleProfileDto updateDto,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var profile = await _chatService.UpdateStyleProfileAsync(updateDto, userId, tenantId, cancellationToken);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating style profile");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}

