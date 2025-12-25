namespace AIProjectManager.Application.DTOs.Chat;

public class ChatQueryDto
{
    public string Question { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? ParentSessionId { get; set; } // For follow-up questions
    public bool IncludeHistory { get; set; } = true; // Include previous chat context
}

