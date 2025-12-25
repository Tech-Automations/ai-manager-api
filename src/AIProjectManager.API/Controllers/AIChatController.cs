using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIChatController : ControllerBase
{
    private readonly IAIChatService _aiChatService;
    private readonly IValidator<AIChatRequestDto> _validator;
    private readonly ILogger<AIChatController> _logger;

    public AIChatController(
        IAIChatService aiChatService,
        IValidator<AIChatRequestDto> validator,
        ILogger<AIChatController> logger)
    {
        _aiChatService = aiChatService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AIChatResponseDto>> Chat([FromBody] AIChatRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var response = await _aiChatService.SendMessageAsync(request, userId, tenantId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI chat request");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}

