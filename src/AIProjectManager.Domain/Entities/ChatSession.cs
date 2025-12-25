using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class ChatSession : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Response { get; set; }
    public string? Sources { get; set; } // JSON array of source references (projects, tasks, etc.)
    public decimal? Confidence { get; set; } // 0.0 to 1.0
    public Guid? ParentSessionId { get; set; } // For follow-up questions
    public string? Model { get; set; } // LLM model used (e.g., "gpt-4")
    public int? TokenCount { get; set; }
    public int? ResponseTimeMs { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual ChatSession? ParentSession { get; set; }
    public virtual ICollection<ChatSession> FollowUps { get; set; } = new List<ChatSession>();
}

