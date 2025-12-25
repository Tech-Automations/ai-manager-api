namespace AIProjectManager.Application.DTOs.Integration;

public class GitRepositoryDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

