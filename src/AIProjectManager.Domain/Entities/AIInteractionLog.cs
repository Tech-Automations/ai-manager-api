using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class AIInteractionLog : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public Guid? TaskItemId { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string? AIResponse { get; set; }
    public string Model { get; set; } = "mock"; // Will be OpenAI model name later
    public int? TokenCount { get; set; }
    public decimal? Cost { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string Status { get; set; } = "Success"; // Success, Error
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual TaskItem? TaskItem { get; set; }
}

