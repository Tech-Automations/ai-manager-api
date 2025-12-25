using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, User
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public virtual ManagerStyleProfile? StyleProfile { get; set; }
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}

