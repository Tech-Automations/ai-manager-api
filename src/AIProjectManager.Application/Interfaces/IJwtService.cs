using AIProjectManager.Domain.Entities;

namespace AIProjectManager.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

