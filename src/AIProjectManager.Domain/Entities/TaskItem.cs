using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo"; // Todo, InProgress, Done, Blocked
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
}

