using System.Diagnostics;
using System.Text.Json;
using AIProjectManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Infrastructure.Services;

public class OpenAIService : ILLMService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;
    private readonly HttpClient _httpClient;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<LLMResponse> GenerateResponseAsync(string prompt, LLMContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var apiKey = _configuration["OpenAI:ApiKey"];
        
        // If no API key configured, return mock response
        if (string.IsNullOrEmpty(apiKey))
        {
            return new LLMResponse
            {
                Content = GenerateMockResponse(prompt, context),
                TokenCount = 0,
                Model = "mock",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Confidence = 0.5m
            };
        }

        try
        {
            var model = context.Model ?? "gpt-4";
            var temperature = context.Temperature ?? 0.7m;
            var maxTokens = context.MaxTokens ?? 1000;

            // Build system prompt
            var systemPrompt = context.SystemPrompt ?? "You are an AI assistant helping project managers with their projects and tasks. Provide clear, concise, and helpful responses.";

            // Build full prompt with context
            var fullPrompt = BuildPromptWithContext(prompt, context, systemPrompt);

            // Prepare OpenAI API request
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = fullPrompt }
                },
                temperature = (float)temperature,
                max_tokens = maxTokens
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

            stopwatch.Stop();

            var aiContent = openAiResponse?.choices?[0]?.message?.content ?? string.Empty;
            var tokenCount = openAiResponse?.usage?.total_tokens ?? 0;

            return new LLMResponse
            {
                Content = aiContent,
                TokenCount = tokenCount,
                Model = model,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Confidence = 0.95m // High confidence for successful API responses
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error calling OpenAI API");
            
            return new LLMResponse
            {
                Content = "I'm sorry, I encountered an error processing your request. Please try again.",
                TokenCount = 0,
                Model = context.Model ?? "error",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Confidence = 0m
            };
        }
    }

    public async Task<T> GenerateStructuredResponseAsync<T>(string prompt, LLMContext context, CancellationToken cancellationToken = default)
    {
        var response = await GenerateResponseAsync(prompt, context, cancellationToken);
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(response.Content);
            return result ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing structured response");
            throw new InvalidOperationException("Failed to parse structured response from LLM", ex);
        }
    }

    private string BuildPromptWithContext(string prompt, LLMContext context, string systemPrompt)
    {
        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine(prompt);

        // Add additional context if provided
        if (context.AdditionalContext.Any())
        {
            contextBuilder.AppendLine("\nAdditional Context:");
            foreach (var kvp in context.AdditionalContext)
            {
                contextBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        return contextBuilder.ToString();
    }

    private string GenerateMockResponse(string prompt, LLMContext context)
    {
        var lowerPrompt = prompt.ToLower();
        
        if (lowerPrompt.Contains("project") || lowerPrompt.Contains("status"))
        {
            return "Based on your project data, I can see you're working on several projects. Would you like more details about a specific project?";
        }
        else if (lowerPrompt.Contains("task") || lowerPrompt.Contains("todo"))
        {
            return "I can help you with task management. Currently, I'm running in mock mode. When OpenAI is configured, I'll provide detailed insights about your tasks.";
        }
        else if (lowerPrompt.Contains("risk") || lowerPrompt.Contains("delay"))
        {
            return "Risk analysis is a key feature. Once OpenAI is configured, I'll analyze your projects for potential risks and delays.";
        }
        else
        {
            return "I'm here to help you manage your projects. I'm currently running in mock mode. Please configure OpenAI API key in appsettings.json to enable full AI capabilities.";
        }
    }

    // Helper classes for OpenAI API response
    private class OpenAIResponse
    {
        public List<Choice>? choices { get; set; }
        public Usage? usage { get; set; }
    }

    private class Choice
    {
        public Message? message { get; set; }
        public string? finish_reason { get; set; }
    }

    private class Message
    {
        public string? content { get; set; }
    }

    private class Usage
    {
        public int total_tokens { get; set; }
    }
}
