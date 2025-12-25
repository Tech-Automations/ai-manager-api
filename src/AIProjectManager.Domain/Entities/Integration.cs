using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class Integration : BaseEntity
{
    public string Type { get; set; } = string.Empty; // AzureDevOps, Jira, GitHub, GitLab, Bitbucket, Slack, Teams
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Configuration { get; set; } // JSON config (encrypted credentials, URLs, etc.)
    public DateTime? LastSyncedAt { get; set; }
    public string Status { get; set; } = "Connected"; // Connected, Disconnected, Error
    public string? ErrorMessage { get; set; }
}

