using System.Text.Json;
using AIProjectManager.Application.DTOs;
using AIProjectManager.Application.DTOs.Auth;
using AIProjectManager.Application.DTOs.Chat;
using AIProjectManager.Domain.Entities;
using AutoMapper;

namespace AIProjectManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => $"{src.Owner.FirstName} {src.Owner.LastName}"));

        // TaskItem mappings
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => 
                src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}" : null));

        // ChatSession mappings
        CreateMap<ChatSession, ChatSessionDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.Sources, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (!string.IsNullOrEmpty(src.Sources))
                {
                    try
                    {
                        dest.Sources = JsonSerializer.Deserialize<List<ChatSourceDto>>(src.Sources) ?? new List<ChatSourceDto>();
                    }
                    catch
                    {
                        dest.Sources = new List<ChatSourceDto>();
                    }
                }
                else
                {
                    dest.Sources = new List<ChatSourceDto>();
                }
            });

        // ManagerStyleProfile mappings
        CreateMap<ManagerStyleProfile, ManagerStyleProfileDto>();
    }
}

