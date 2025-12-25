using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class ExternalWorkItem : BaseEntity
{
    public Guid IntegrationId { get; set; }
    public Guid? TaskItemId { get; set; }
    public string ExternalId { get; set; } = string.Empty; // ADO/Jira work item ID
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? ExternalUrl { get; set; }
    
    // Navigation properties
    public virtual Integration Integration { get; set; } = null!;
    public virtual TaskItem? TaskItem { get; set; }
}

