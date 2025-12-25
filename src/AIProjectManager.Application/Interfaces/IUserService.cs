using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.DTOs.Auth;

namespace AIProjectManager.Application.Interfaces;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

