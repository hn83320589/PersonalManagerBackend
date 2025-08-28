using AutoMapper;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.DTOs.User;
using PersonalManagerAPI.DTOs.PersonalProfile;
using PersonalManagerAPI.DTOs.Education;
using PersonalManagerAPI.DTOs.Skill;
using PersonalManagerAPI.DTOs.WorkExperience;
using PersonalManagerAPI.DTOs.Portfolio;
using PersonalManagerAPI.DTOs.CalendarEvent;
using PersonalManagerAPI.DTOs.TodoItem;
using PersonalManagerAPI.DTOs.WorkTask;
using PersonalManagerAPI.DTOs.BlogPost;
using PersonalManagerAPI.DTOs.GuestBookEntry;
using PersonalManagerAPI.DTOs.ContactMethod;
using PersonalManagerAPI.DTOs;

namespace PersonalManagerAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateUserMappings();
        CreatePersonalProfileMappings();
        CreateEducationMappings();
        CreateSkillMappings();
        CreateWorkExperienceMappings();
        CreatePortfolioMappings();
        CreateCalendarEventMappings();
        CreateTodoItemMappings();
        CreateWorkTaskMappings();
        CreateBlogPostMappings();
        CreateGuestBookEntryMappings();
        CreateContactMethodMappings();
        CreateRbacMappings();
    }

    private void CreateUserMappings()
    {
        // User 對映
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // 密碼會在服務中處理
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, UserResponseDto>();
    }

    private void CreatePersonalProfileMappings()
    {
        // PersonalProfile 對映
        CreateMap<CreatePersonalProfileDto, PersonalProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdatePersonalProfileDto, PersonalProfile>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<PersonalProfile, PersonalProfileResponseDto>();
    }

    private void CreateEducationMappings()
    {
        // Education 對映
        CreateMap<CreateEducationDto, Education>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateEducationDto, Education>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Education, EducationResponseDto>();
    }

    private void CreateSkillMappings()
    {
        // Skill 對映
        CreateMap<CreateSkillDto, Models.Skill>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateSkillDto, Models.Skill>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.Skill, SkillResponseDto>();
    }

    private void CreateWorkExperienceMappings()
    {
        // WorkExperience 對映
        CreateMap<CreateWorkExperienceDto, Models.WorkExperience>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateWorkExperienceDto, Models.WorkExperience>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.WorkExperience, WorkExperienceResponseDto>();
    }

    private void CreatePortfolioMappings()
    {
        // Portfolio 對映
        CreateMap<CreatePortfolioDto, Models.Portfolio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdatePortfolioDto, Models.Portfolio>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.Portfolio, PortfolioResponseDto>();
    }

    private void CreateCalendarEventMappings()
    {
        // CalendarEvent 對映
        CreateMap<CreateCalendarEventDto, Models.CalendarEvent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateCalendarEventDto, Models.CalendarEvent>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.CalendarEvent, CalendarEventResponseDto>();
    }

    private void CreateTodoItemMappings()
    {
        // TodoItem 對映
        CreateMap<CreateTodoItemDto, Models.TodoItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseTaskStatus(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => ParseTodoPriority(src.Priority)));

        CreateMap<UpdateTodoItemDto, Models.TodoItem>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseTaskStatusNullable(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => ParseTodoPriorityNullable(src.Priority)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.TodoItem, TodoItemResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
    }

    private static Models.TaskStatus ParseTaskStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return Models.TaskStatus.Pending;
        
        return Enum.TryParse<Models.TaskStatus>(status, true, out var result) 
            ? result 
            : Models.TaskStatus.Pending;
    }

    private static Models.TaskStatus? ParseTaskStatusNullable(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return null;
        
        return Enum.TryParse<Models.TaskStatus>(status, true, out var result) 
            ? result 
            : null;
    }

    private static TodoPriority ParseTodoPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return TodoPriority.Medium;
        
        return Enum.TryParse<TodoPriority>(priority, true, out var result) 
            ? result 
            : TodoPriority.Medium;
    }

    private static TodoPriority? ParseTodoPriorityNullable(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return null;
        
        return Enum.TryParse<TodoPriority>(priority, true, out var result) 
            ? result 
            : null;
    }

    private void CreateWorkTaskMappings()
    {
        // WorkTask 對映
        CreateMap<CreateWorkTaskDto, Models.WorkTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseWorkTaskStatus(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => ParseTaskPriority(src.Priority)));

        CreateMap<UpdateWorkTaskDto, Models.WorkTask>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseWorkTaskStatusNullable(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => ParseTaskPriorityNullable(src.Priority)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.WorkTask, WorkTaskResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
    }

    private static Models.TaskStatus ParseWorkTaskStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return Models.TaskStatus.Pending;
        
        return Enum.TryParse<Models.TaskStatus>(status, true, out var result) 
            ? result 
            : Models.TaskStatus.Pending;
    }

    private static Models.TaskStatus? ParseWorkTaskStatusNullable(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return null;
        
        return Enum.TryParse<Models.TaskStatus>(status, true, out var result) 
            ? result 
            : null;
    }

    private static TaskPriority ParseTaskPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return TaskPriority.Medium;
        
        return Enum.TryParse<TaskPriority>(priority, true, out var result) 
            ? result 
            : TaskPriority.Medium;
    }

    private static TaskPriority? ParseTaskPriorityNullable(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return null;
        
        return Enum.TryParse<TaskPriority>(priority, true, out var result) 
            ? result 
            : null;
    }

    private void CreateBlogPostMappings()
    {
        // BlogPost 對映
        CreateMap<CreateBlogPostDto, Models.BlogPost>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedDate, opt => opt.Ignore());

        CreateMap<UpdateBlogPostDto, Models.BlogPost>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Models.BlogPost, BlogPostResponseDto>();
    }

    private void CreateGuestBookEntryMappings()
    {
        // GuestBookEntry 對映
        CreateMap<CreateGuestBookEntryDto, GuestBookEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.Ignore())
            .ForMember(dest => dest.UserAgent, opt => opt.Ignore())
            .ForMember(dest => dest.IsApproved, opt => opt.Ignore());

        CreateMap<UpdateGuestBookEntryDto, GuestBookEntry>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<GuestBookEntry, GuestBookEntryResponseDto>()
            .ForMember(dest => dest.Replies, opt => opt.Ignore()) // 將在服務中手動填充
            .ForMember(dest => dest.ReplyCount, opt => opt.Ignore()); // 將在服務中手動填充
    }

    private void CreateContactMethodMappings()
    {
        // ContactMethod 對映
        CreateMap<CreateContactMethodDto, ContactMethod>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<UpdateContactMethodDto, ContactMethod>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ContactMethod, ContactMethodResponseDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));
    }

    private void CreateRbacMappings()
    {
        // Role 對映
        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RolePermissions, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

        CreateMap<UpdateRoleDto, Role>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Role, RoleResponseDto>()
            .ForMember(dest => dest.PermissionCount, opt => opt.MapFrom(src => src.RolePermissions.Count))
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.UserRoles.Count))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission)));

        // Permission 對映
        CreateMap<CreatePermissionDto, Permission>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

        CreateMap<UpdatePermissionDto, Permission>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Permission, PermissionResponseDto>();

        // UserRole 對映
        CreateMap<AssignUserRoleDto, UserRole>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<UserRole, UserRoleResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.RoleDisplayName, opt => opt.MapFrom(src => src.Role.DisplayName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}