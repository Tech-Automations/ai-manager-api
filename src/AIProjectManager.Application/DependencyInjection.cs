using AIProjectManager.Application.Interfaces;
using AIProjectManager.Application.Mappings;
using AIProjectManager.Application.Services;
using AIProjectManager.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AIProjectManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<IAIChatService, AIChatService>();

        return services;
    }
}

