namespace AIProjectManager.Application.DTOs.Chat;

public class ManagerStyleProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tone { get; set; } = "Direct"; // Direct, Soft, Technical
    public bool PreferBullets { get; set; } = true;
    public bool IncludeRisksByDefault { get; set; } = true;
    public bool AutoCreateTasks { get; set; } = false;
}

public class UpdateManagerStyleProfileDto
{
    public string? Tone { get; set; }
    public bool? PreferBullets { get; set; }
    public bool? IncludeRisksByDefault { get; set; }
    public bool? AutoCreateTasks { get; set; }
}

