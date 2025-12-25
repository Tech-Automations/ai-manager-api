namespace AIProjectManager.Application.DTOs;

public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}

