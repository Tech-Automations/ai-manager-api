using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class GitRepository : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // GitHub, GitLab, Bitbucket
    public string? AccessToken { get; set; } // Encrypted
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncedAt { get; set; }
    
    // Navigation properties
    public virtual Project? Project { get; set; }
    public virtual ICollection<GitCommit> Commits { get; set; } = new List<GitCommit>();
}

