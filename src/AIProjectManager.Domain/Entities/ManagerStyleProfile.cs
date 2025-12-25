using AIProjectManager.Domain.Common;

namespace AIProjectManager.Domain.Entities;

public class ManagerStyleProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string Tone { get; set; } = "Direct"; // Direct, Soft, Technical
    public bool PreferBullets { get; set; } = true;
    public bool IncludeRisksByDefault { get; set; } = true;
    public bool AutoCreateTasks { get; set; } = false;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}

