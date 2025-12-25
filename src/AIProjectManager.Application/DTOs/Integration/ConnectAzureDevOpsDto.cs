namespace AIProjectManager.Application.DTOs.Integration;

public class ConnectAzureDevOpsDto
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
}

