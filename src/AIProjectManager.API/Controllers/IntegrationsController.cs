using AIProjectManager.Application.DTOs.Integration;
using AIProjectManager.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProjectManager.API.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationService _integrationService;
    private readonly IValidator<ConnectAzureDevOpsDto> _connectAdoValidator;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IIntegrationService integrationService,
        IValidator<ConnectAzureDevOpsDto> connectAdoValidator,
        ILogger<IntegrationsController> logger)
    {
        _integrationService = integrationService;
        _connectAdoValidator = connectAdoValidator;
        _logger = logger;
    }

    [HttpPost("azure-devops/connect")]
    public async Task<ActionResult<IntegrationDto>> ConnectAzureDevOps([FromBody] ConnectAzureDevOpsDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _connectAdoValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var integration = await _integrationService.ConnectAzureDevOpsAsync(request, userId, tenantId, cancellationToken);
            return Ok(integration);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting Azure DevOps");
            return StatusCode(500, new { error = "An error occurred while connecting Azure DevOps" });
        }
    }

    [HttpGet("azure-devops/status")]
    public async Task<ActionResult<IntegrationDto>> GetAzureDevOpsStatus(CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var integrations = await _integrationService.GetIntegrationsAsync(tenantId, cancellationToken);
            var adoIntegration = integrations.FirstOrDefault(i => i.Type == "AzureDevOps");
            
            if (adoIntegration == null)
            {
                return NotFound(new { error = "Azure DevOps integration not found" });
            }

            return Ok(adoIntegration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Azure DevOps status");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPost("azure-devops/sync")]
    public async Task<ActionResult<IntegrationDto>> SyncAzureDevOps(CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var integrations = await _integrationService.GetIntegrationsAsync(tenantId, cancellationToken);
            var adoIntegration = integrations.FirstOrDefault(i => i.Type == "AzureDevOps" && i.IsActive);
            
            if (adoIntegration == null)
            {
                return NotFound(new { error = "Active Azure DevOps integration not found" });
            }

            var result = await _integrationService.SyncAzureDevOpsAsync(adoIntegration.Id, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Azure DevOps");
            return StatusCode(500, new { error = "An error occurred while syncing Azure DevOps" });
        }
    }

    [HttpGet("azure-devops/work-items")]
    public async Task<ActionResult<IEnumerable<ExternalWorkItemDto>>> GetWorkItems(
        [FromQuery] Guid? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var integrations = await _integrationService.GetIntegrationsAsync(tenantId, cancellationToken);
            var adoIntegration = integrations.FirstOrDefault(i => i.Type == "AzureDevOps" && i.IsActive);
            
            if (adoIntegration == null)
            {
                return NotFound(new { error = "Active Azure DevOps integration not found" });
            }

            var workItems = await _integrationService.GetWorkItemsAsync(adoIntegration.Id, tenantId, projectId, cancellationToken);
            return Ok(workItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work items");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPost("azure-devops/work-items/{workItemId}/link")]
    public async Task<ActionResult<ExternalWorkItemDto>> LinkWorkItemToTask(
        Guid workItemId,
        [FromBody] Guid taskItemId,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var result = await _integrationService.LinkWorkItemToTaskAsync(workItemId, taskItemId, tenantId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work item to task");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IntegrationDto>>> GetIntegrations(CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var integrations = await _integrationService.GetIntegrationsAsync(tenantId, cancellationToken);
            return Ok(integrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integrations");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPut("{id}/disconnect")]
    public async Task<ActionResult> DisconnectIntegration(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            await _integrationService.DisconnectIntegrationAsync(id, tenantId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting integration");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}

