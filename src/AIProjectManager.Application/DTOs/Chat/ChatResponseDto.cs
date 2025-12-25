namespace AIProjectManager.Application.DTOs.Chat;

public class ChatResponseDto
{
    public Guid SessionId { get; set; }
    public string Response { get; set; } = string.Empty;
    public decimal? Confidence { get; set; } // 0.0 to 1.0
    public List<ChatSourceDto> Sources { get; set; } = new List<ChatSourceDto>();
    public string? Model { get; set; }
    public int? TokenCount { get; set; }
    public int? ResponseTimeMs { get; set; }
}

public class ChatSourceDto
{
    public string Type { get; set; } = string.Empty; // Project, Task, Meeting, etc.
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

