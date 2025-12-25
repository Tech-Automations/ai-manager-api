namespace AIProjectManager.Application.DTOs.Integration;

public class GitCommitDto
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    public Guid? ProjectId { get; set; }
    public string CommitHash { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime CommittedAt { get; set; }
    public string Branch { get; set; } = string.Empty;
}

