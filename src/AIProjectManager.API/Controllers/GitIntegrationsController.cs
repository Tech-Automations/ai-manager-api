using AIProjectManager.Application.DTOs.Integration;
using AIProjectManager.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProjectManager.API.Controllers;

[ApiController]
[Route("api/integrations/git")]
[Authorize]
public class GitIntegrationsController : ControllerBase
{
    private readonly IGitIntegrationService _gitService;
    private readonly IValidator<ConnectGitRepositoryDto> _connectGitValidator;
    private readonly ILogger<GitIntegrationsController> _logger;

    public GitIntegrationsController(
        IGitIntegrationService gitService,
        IValidator<ConnectGitRepositoryDto> connectGitValidator,
        ILogger<GitIntegrationsController> logger)
    {
        _gitService = gitService;
        _connectGitValidator = connectGitValidator;
        _logger = logger;
    }

    [HttpPost("connect")]
    public async Task<ActionResult<GitRepositoryDto>> ConnectRepository([FromBody] ConnectGitRepositoryDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _connectGitValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var repository = await _gitService.ConnectRepositoryAsync(request, userId, tenantId, cancellationToken);
            return Ok(repository);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting Git repository");
            return StatusCode(500, new { error = "An error occurred while connecting repository" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GitRepositoryDto>>> GetRepositories(
        [FromQuery] Guid? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var repositories = await _gitService.GetRepositoriesAsync(tenantId, projectId, cancellationToken);
            return Ok(repositories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repositories");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GitRepositoryDto>> GetRepository(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var repository = await _gitService.GetRepositoryAsync(id, tenantId, cancellationToken);
            return Ok(repository);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repository");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPost("{id}/sync")]
    public async Task<ActionResult<GitRepositoryDto>> SyncRepository(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var repository = await _gitService.SyncRepositoryAsync(id, tenantId, cancellationToken);
            return Ok(repository);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing repository");
            return StatusCode(500, new { error = "An error occurred while syncing repository" });
        }
    }

    [HttpGet("{id}/commits")]
    public async Task<ActionResult<IEnumerable<GitCommitDto>>> GetCommits(
        Guid id,
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var commits = await _gitService.GetCommitsAsync(id, tenantId, projectId, dateFrom, cancellationToken);
            return Ok(commits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commits");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DisconnectRepository(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            await _gitService.DisconnectRepositoryAsync(id, tenantId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting repository");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPost("webhook/{provider}")]
    [AllowAnonymous] // Webhooks come from external sources
    public async Task<ActionResult> HandleWebhook(string provider, [FromBody] object payload, CancellationToken cancellationToken)
    {
        try
        {
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            await _gitService.HandleWebhookAsync(provider, payloadJson, cancellationToken);
            return Ok(new { message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {Provider}", provider);
            return StatusCode(500, new { error = "An error occurred processing webhook" });
        }
    }
}

