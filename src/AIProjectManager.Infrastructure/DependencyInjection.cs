using AIProjectManager.Application.Interfaces;
using AIProjectManager.Domain.Entities;
using AIProjectManager.Domain.Interfaces;
using AIProjectManager.Infrastructure.Data;
using AIProjectManager.Infrastructure.Repositories;
using AIProjectManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIProjectManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("AIProjectManager.Infrastructure")));

        // Repositories
        services.AddScoped<IRepository<Tenant>, TenantRepository>();
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IRepository<Project>, ProjectRepository>();
        services.AddScoped<IRepository<TaskItem>, TaskItemRepository>();
        services.AddScoped<IRepository<AIInteractionLog>, Repository<AIInteractionLog>>();
        services.AddScoped<IRepository<ChatSession>, ChatSessionRepository>();
        services.AddScoped<IRepository<ManagerStyleProfile>, ManagerStyleProfileRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<AIProjectManager.Application.Interfaces.ILLMService, OpenAIService>();

        return services;
    }
}

