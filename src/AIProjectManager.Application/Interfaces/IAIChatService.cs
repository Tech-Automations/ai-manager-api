using AIProjectManager.Application.DTOs;

namespace AIProjectManager.Application.Interfaces;

public interface IAIChatService
{
    Task<AIChatResponseDto> SendMessageAsync(AIChatRequestDto request, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

