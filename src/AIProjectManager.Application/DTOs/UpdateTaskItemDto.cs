namespace AIProjectManager.Application.DTOs;

public class UpdateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}

