namespace AIProjectManager.Application.DTOs;

public class AIChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public Guid InteractionLogId { get; set; }
}

