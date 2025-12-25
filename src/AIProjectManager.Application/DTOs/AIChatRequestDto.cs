namespace AIProjectManager.Application.DTOs;

public class AIChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? TaskItemId { get; set; }
}

