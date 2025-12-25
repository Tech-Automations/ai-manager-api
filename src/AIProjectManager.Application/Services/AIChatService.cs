using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class AIChatService : IAIChatService
{
    private readonly IRepository<AIInteractionLog> _interactionLogRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly ILogger<AIChatService> _logger;

    public AIChatService(
        IRepository<AIInteractionLog> interactionLogRepository,
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        ILogger<AIChatService> logger)
    {
        _interactionLogRepository = interactionLogRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<AIChatResponseDto> SendMessageAsync(AIChatRequestDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Verify project exists if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, tenantId, cancellationToken);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }
        }

        // Verify task exists if provided
        if (request.TaskItemId.HasValue)
        {
            var task = await _taskRepository.GetByIdAsync(request.TaskItemId.Value, tenantId, cancellationToken);
            if (task == null)
            {
                throw new KeyNotFoundException("Task not found");
            }
        }

        // Create interaction log
        var interactionLog = new AIInteractionLog
        {
            UserPrompt = request.Message,
            ProjectId = request.ProjectId,
            TaskItemId = request.TaskItemId,
            TenantId = tenantId,
            Model = "mock",
            Status = "Success",
            CreatedBy = userId
        };

        // Mock AI response (to be replaced with OpenAI integration later)
        var mockResponse = GenerateMockResponse(request.Message);

        interactionLog.AIResponse = mockResponse;
        interactionLog.ResponseTimeMs = new Random().Next(500, 2000);
        interactionLog.TokenCount = mockResponse.Split(' ').Length * 2; // Rough estimate

        interactionLog = await _interactionLogRepository.AddAsync(interactionLog, cancellationToken);

        return new AIChatResponseDto
        {
            Response = mockResponse,
            InteractionLogId = interactionLog.Id
        };
    }

    private string GenerateMockResponse(string userMessage)
    {
        // Simple mock responses based on keywords
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("task") || lowerMessage.Contains("todo"))
        {
            return "I can help you manage tasks. Based on your project requirements, I suggest creating a detailed task breakdown. Would you like me to help you create a task list?";
        }
        else if (lowerMessage.Contains("project") || lowerMessage.Contains("plan"))
        {
            return "I understand you're working on a project. I can help you with project planning, task organization, and progress tracking. What specific aspect would you like assistance with?";
        }
        else if (lowerMessage.Contains("help") || lowerMessage.Contains("how"))
        {
            return "I'm here to help you manage your AI Project Manager. I can assist with creating tasks, organizing projects, tracking progress, and providing insights. What would you like to do?";
        }
        else
        {
            return "Thank you for your message. I'm currently running in mock mode. When OpenAI integration is configured, I'll be able to provide more detailed and context-aware responses. How can I assist you with your project management needs?";
        }
    }
}

