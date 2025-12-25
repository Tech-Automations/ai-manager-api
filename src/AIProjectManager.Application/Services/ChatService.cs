using System.Text.Json;
using AIProjectManager.Application.DTOs.Chat;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class ChatService : IChatService
{
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly IRepository<ManagerStyleProfile> _styleProfileRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILLMService _llmService;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IRepository<ChatSession> chatSessionRepository,
        IRepository<ManagerStyleProfile> styleProfileRepository,
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        IRepository<User> userRepository,
        ILLMService llmService,
        IMapper mapper,
        ILogger<ChatService> logger)
    {
        _chatSessionRepository = chatSessionRepository;
        _styleProfileRepository = styleProfileRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _llmService = llmService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ChatResponseDto> SendQueryAsync(ChatQueryDto query, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId, tenantId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify project exists if provided
        if (query.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(query.ProjectId.Value, tenantId, cancellationToken);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {query.ProjectId.Value} not found");
            }
        }

        // Verify parent session exists if provided
        if (query.ParentSessionId.HasValue)
        {
            var parentSession = await _chatSessionRepository.GetByIdAsync(query.ParentSessionId.Value, tenantId, cancellationToken);
            if (parentSession == null || parentSession.UserId != userId)
            {
                throw new KeyNotFoundException($"Parent chat session with ID {query.ParentSessionId.Value} not found");
            }
        }

        // Get or create style profile
        var styleProfile = await GetOrCreateStyleProfileAsync(userId, tenantId, cancellationToken);

        // Build context (RAG - Retrieve relevant data)
        var context = await BuildContextAsync(query, userId, tenantId, cancellationToken);

        // Build prompt with style preferences
        var prompt = await BuildPromptAsync(query.Question, context, styleProfile, query.IncludeHistory, query.ParentSessionId, userId, tenantId, cancellationToken);

        // Prepare LLM context
        var llmContext = new LLMContext
        {
            ProjectId = query.ProjectId,
            TenantId = tenantId,
            Model = "gpt-4",
            Temperature = 0.7m,
            MaxTokens = 1500,
            SystemPrompt = BuildSystemPrompt(styleProfile),
            AdditionalContext = context.AdditionalContext
        };

        // Call LLM
        var llmResponse = await _llmService.GenerateResponseAsync(prompt, llmContext, cancellationToken);

        stopwatch.Stop();

        // Extract sources from context
        var sources = ExtractSources(context);

        // Save chat session
        var chatSession = new ChatSession
        {
            UserId = userId,
            TenantId = tenantId,
            ProjectId = query.ProjectId,
            Question = query.Question,
            Response = llmResponse.Content,
            Confidence = llmResponse.Confidence,
            Sources = JsonSerializer.Serialize(sources),
            ParentSessionId = query.ParentSessionId,
            Model = llmResponse.Model,
            TokenCount = llmResponse.TokenCount,
            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
            CreatedBy = userId
        };

        chatSession = await _chatSessionRepository.AddAsync(chatSession, cancellationToken);

        return new ChatResponseDto
        {
            SessionId = chatSession.Id,
            Response = llmResponse.Content,
            Confidence = llmResponse.Confidence,
            Sources = sources,
            Model = llmResponse.Model,
            TokenCount = llmResponse.TokenCount,
            ResponseTimeMs = llmResponse.ResponseTimeMs
        };
    }

    public async Task<IEnumerable<ChatSessionDto>> GetChatHistoryAsync(Guid userId, Guid tenantId, Guid? projectId = null, DateTime? dateFrom = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        // Get all sessions for tenant, filter by user and criteria
        var allSessions = await _chatSessionRepository.GetAllAsync(tenantId, cancellationToken);
        var filtered = allSessions
            .Where(c => c.UserId == userId && c.ParentSessionId == null);

        if (projectId.HasValue)
        {
            filtered = filtered.Where(c => c.ProjectId == projectId);
        }

        if (dateFrom.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt >= dateFrom.Value);
        }

        var sessions = filtered
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToList();

        return _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);
    }

    public async Task<ChatSessionDto?> GetChatSessionAsync(Guid sessionId, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(sessionId, tenantId, cancellationToken);
        
        if (session == null || session.UserId != userId)
        {
            return null;
        }

        return _mapper.Map<ChatSessionDto>(session);
    }

    public async Task DeleteChatSessionAsync(Guid sessionId, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(sessionId, tenantId, cancellationToken);

        if (session == null || session.UserId != userId)
        {
            throw new KeyNotFoundException("Chat session not found");
        }

        await _chatSessionRepository.DeleteAsync(session, cancellationToken);
    }

    public async Task<ManagerStyleProfileDto> GetStyleProfileAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateStyleProfileAsync(userId, tenantId, cancellationToken);
        return _mapper.Map<ManagerStyleProfileDto>(profile);
    }

    public async Task<ManagerStyleProfileDto> UpdateStyleProfileAsync(UpdateManagerStyleProfileDto updateDto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateStyleProfileAsync(userId, tenantId, cancellationToken);

        if (updateDto.Tone != null)
            profile.Tone = updateDto.Tone;
        if (updateDto.PreferBullets.HasValue)
            profile.PreferBullets = updateDto.PreferBullets.Value;
        if (updateDto.IncludeRisksByDefault.HasValue)
            profile.IncludeRisksByDefault = updateDto.IncludeRisksByDefault.Value;
        if (updateDto.AutoCreateTasks.HasValue)
            profile.AutoCreateTasks = updateDto.AutoCreateTasks.Value;

        profile.UpdatedBy = userId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _styleProfileRepository.UpdateAsync(profile, cancellationToken);

        return _mapper.Map<ManagerStyleProfileDto>(profile);
    }

    private async Task<ManagerStyleProfile> GetOrCreateStyleProfileAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        // Get all profiles for tenant, find by userId
        var allProfiles = await _styleProfileRepository.GetAllAsync(tenantId, cancellationToken);
        var existing = allProfiles.FirstOrDefault(p => p.UserId == userId);
        
        if (existing != null)
        {
            return existing;
        }

        // Create default profile
        var newProfile = new ManagerStyleProfile
        {
            UserId = userId,
            TenantId = tenantId,
            Tone = "Direct",
            PreferBullets = true,
            IncludeRisksByDefault = true,
            AutoCreateTasks = false,
            CreatedBy = userId
        };

        return await _styleProfileRepository.AddAsync(newProfile, cancellationToken);
    }

    private async Task<ChatContext> BuildContextAsync(ChatQueryDto query, Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var context = new ChatContext
        {
            AdditionalContext = new Dictionary<string, object>()
        };

        // Get projects
        var projects = await _projectRepository.GetAllAsync(tenantId, cancellationToken);
        context.Projects = projects.ToList();

        // Get tasks
        var tasks = await _taskRepository.GetAllAsync(tenantId, cancellationToken);
        context.Tasks = tasks.ToList();

        // If specific project requested, get detailed info
        if (query.ProjectId.HasValue)
        {
            var project = context.Projects.FirstOrDefault(p => p.Id == query.ProjectId.Value);
            if (project != null)
            {
                context.AdditionalContext["ProjectName"] = project.Name;
                context.AdditionalContext["ProjectStatus"] = project.Status;
                context.AdditionalContext["ProjectDescription"] = project.Description ?? "";
            }

            var projectTasks = context.Tasks.Where(t => t.ProjectId == query.ProjectId.Value).ToList();
            context.AdditionalContext["ProjectTaskCount"] = projectTasks.Count;
            context.AdditionalContext["ProjectTasks"] = projectTasks.Select(t => new { t.Title, t.Status, t.Priority }).ToList();
        }

        // Build context summary for LLM
        context.AdditionalContext["TotalProjects"] = context.Projects.Count;
        context.AdditionalContext["TotalTasks"] = context.Tasks.Count;
        context.AdditionalContext["ActiveProjects"] = context.Projects.Count(p => p.Status == "Active");

        return context;
    }

    private async Task<string> BuildPromptAsync(string question, ChatContext context, ManagerStyleProfile styleProfile, bool includeHistory, Guid? parentSessionId, Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var promptBuilder = new System.Text.StringBuilder();

        // Add project context
        if (context.Projects.Any())
        {
            promptBuilder.AppendLine("Projects:");
            foreach (var project in context.Projects.Take(10)) // Limit to prevent token overflow
            {
                promptBuilder.AppendLine($"- {project.Name} (Status: {project.Status})");
            }
        }

        // Add task context
        if (context.Tasks.Any())
        {
            promptBuilder.AppendLine("\nTasks:");
            foreach (var task in context.Tasks.Take(20))
            {
                promptBuilder.AppendLine($"- {task.Title} (Status: {task.Status}, Priority: {task.Priority})");
            }
        }

        // Add chat history if requested
        if (includeHistory && parentSessionId.HasValue)
        {
            var parentSession = await _chatSessionRepository.GetByIdAsync(parentSessionId.Value, tenantId, cancellationToken);
            if (parentSession != null && parentSession.UserId == userId)
            {
                promptBuilder.AppendLine($"\nPrevious question: {parentSession.Question}");
                promptBuilder.AppendLine($"Previous response: {parentSession.Response}");
            }
        }
        else if (includeHistory)
        {
            var allSessions = await _chatSessionRepository.GetAllAsync(tenantId, cancellationToken);
            var recentSessions = allSessions
                .Where(c => c.UserId == userId && c.TenantId == tenantId && c.ParentSessionId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .ToList();

            if (recentSessions.Any())
            {
                promptBuilder.AppendLine("\nRecent conversation history:");
                foreach (var session in recentSessions)
                {
                    promptBuilder.AppendLine($"Q: {session.Question}");
                    promptBuilder.AppendLine($"A: {session.Response?.Substring(0, Math.Min(200, session.Response?.Length ?? 0))}...");
                }
            }
        }

        promptBuilder.AppendLine($"\nUser Question: {question}");

        return promptBuilder.ToString();
    }

    private string BuildSystemPrompt(ManagerStyleProfile styleProfile)
    {
        var systemPrompt = "You are an AI assistant helping project managers with their projects and tasks. ";
        
        systemPrompt += $"Your communication style should be {styleProfile.Tone.ToLower()}. ";
        
        if (styleProfile.PreferBullets)
        {
            systemPrompt += "Prefer bullet points in your responses. ";
        }
        
        if (styleProfile.IncludeRisksByDefault)
        {
            systemPrompt += "Always include potential risks or concerns when relevant. ";
        }

        systemPrompt += "Provide clear, concise, and actionable responses based on the project data provided.";

        return systemPrompt;
    }

    private List<ChatSourceDto> ExtractSources(ChatContext context)
    {
        var sources = new List<ChatSourceDto>();

        foreach (var project in context.Projects.Take(5))
        {
            sources.Add(new ChatSourceDto
            {
                Type = "Project",
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            });
        }

        foreach (var task in context.Tasks.Take(5))
        {
            sources.Add(new ChatSourceDto
            {
                Type = "Task",
                Id = task.Id,
                Name = task.Title,
                Description = task.Description
            });
        }

        return sources;
    }

    private class ChatContext
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    }
}

