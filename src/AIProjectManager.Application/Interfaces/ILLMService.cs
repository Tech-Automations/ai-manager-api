namespace AIProjectManager.Application.Interfaces;

public class LLMContext
{
    public Guid? ProjectId { get; set; }
    public Guid? TenantId { get; set; }
    public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    public string Model { get; set; } = "gpt-4";
    public int? MaxTokens { get; set; }
    public decimal? Temperature { get; set; } = 0.7m;
    public string? SystemPrompt { get; set; }
}

public class LLMResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public string Model { get; set; } = string.Empty;
    public int ResponseTimeMs { get; set; }
    public decimal? Confidence { get; set; }
}

public interface ILLMService
{
    Task<LLMResponse> GenerateResponseAsync(string prompt, LLMContext context, CancellationToken cancellationToken = default);
    Task<T> GenerateStructuredResponseAsync<T>(string prompt, LLMContext context, CancellationToken cancellationToken = default);
}

