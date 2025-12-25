namespace AIProjectManager.Application.DTOs.Chat;

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Response { get; set; }
    public decimal? Confidence { get; set; }
    public List<ChatSourceDto> Sources { get; set; } = new List<ChatSourceDto>();
    public Guid? ParentSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Model { get; set; }
}

