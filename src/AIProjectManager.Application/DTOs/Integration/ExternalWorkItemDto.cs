namespace AIProjectManager.Application.DTOs.Integration;

public class ExternalWorkItemDto
{
    public Guid Id { get; set; }
    public Guid IntegrationId { get; set; }
    public Guid? TaskItemId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? ExternalUrl { get; set; }
}

