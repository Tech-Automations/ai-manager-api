namespace AIProjectManager.Application.DTOs.Integration;

public class ConnectGitRepositoryDto
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // GitHub, GitLab, Bitbucket
    public string AccessToken { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
}

