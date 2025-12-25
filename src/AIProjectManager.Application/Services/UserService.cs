using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.DTOs.Auth;
using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AIProjectManager.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<Tenant> tenantRepository,
        IRepository<User> userRepository,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Check if tenant with subdomain exists (Tenant doesn't need tenantId filter)
        var existingTenant = (await _tenantRepository.FindAsync(t => t.Subdomain == request.Subdomain, Guid.Empty, cancellationToken))
            .FirstOrDefault();

        Tenant tenant;
        if (existingTenant == null)
        {
            // Create new tenant
            var tenantId = Guid.NewGuid();
            tenant = new Tenant
            {
                Id = tenantId,
                Name = request.TenantName,
                Subdomain = request.Subdomain,
                IsActive = true,
                TenantId = tenantId // Tenant's own ID is its TenantId
            };
            tenant = await _tenantRepository.AddAsync(tenant, cancellationToken);
        }
        else
        {
            tenant = existingTenant;
        }

        // Check if user with email exists in this tenant
        var existingUser = (await _userRepository.FindAsync(u => u.Email == request.Email, tenant.Id, cancellationToken))
            .FirstOrDefault();

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = tenant.Id,
            Role = "Admin" // First user is admin
        };

        user = await _userRepository.AddAsync(user, cancellationToken);

        var token = _jwtService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find user by email (we need to search across all tenants, then filter)
        // Note: In production, you might want to optimize this query
        var allTenants = await _tenantRepository.GetAllAsync(Guid.Empty, cancellationToken);
        
        User? user = null;
        Tenant? tenant = null;

        foreach (var t in allTenants)
        {
            var users = await _userRepository.FindAsync(u => u.Email == request.Email, t.Id, cancellationToken);
            var foundUser = users.FirstOrDefault();
            if (foundUser != null)
            {
                user = foundUser;
                tenant = t;
                break;
            }
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        if (tenant != null && !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Tenant account is inactive");
        }

        var token = _jwtService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, tenantId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserDto>(user);
    }
}

