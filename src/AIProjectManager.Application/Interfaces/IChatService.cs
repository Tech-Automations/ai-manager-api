using AIProjectManager.Application.DTOs.Chat;

namespace AIProjectManager.Application.Interfaces;

public interface IChatService
{
    Task<ChatResponseDto> SendQueryAsync(ChatQueryDto query, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatSessionDto>> GetChatHistoryAsync(Guid userId, Guid tenantId, Guid? projectId = null, DateTime? dateFrom = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<ChatSessionDto?> GetChatSessionAsync(Guid sessionId, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task DeleteChatSessionAsync(Guid sessionId, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ManagerStyleProfileDto> GetStyleProfileAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ManagerStyleProfileDto> UpdateStyleProfileAsync(UpdateManagerStyleProfileDto updateDto, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

