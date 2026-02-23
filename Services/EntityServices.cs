using PersonalManager.Api.DTOs;
using PersonalManager.Api.Mappings;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;

namespace PersonalManager.Api.Services;

// ===== User Service =====
public interface IUserService : ICrudService<User, CreateUserDto, UpdateUserDto, UserResponse> { }

public class UserService : CrudService<User, CreateUserDto, UpdateUserDto, UserResponse>, IUserService
{
    public UserService(IRepository<User> repo) : base(repo) { }
    protected override User MapToEntity(CreateUserDto dto)
    {
        var entity = dto.ToEntity();
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        return entity;
    }
    protected override UserResponse MapToResponse(User entity) => entity.ToResponse();
    protected override void ApplyUpdate(User entity, UpdateUserDto dto) => entity.ApplyUpdate(dto);
}

// ===== Profile Service =====
public interface IProfileService : ICrudService<PersonalProfile, CreateProfileDto, UpdateProfileDto, ProfileResponse>
{
    Task<ProfileResponse?> GetByUserIdAsync(int userId);
}

public class ProfileService : CrudService<PersonalProfile, CreateProfileDto, UpdateProfileDto, ProfileResponse>, IProfileService
{
    public ProfileService(IRepository<PersonalProfile> repo) : base(repo) { }
    protected override PersonalProfile MapToEntity(CreateProfileDto dto) => dto.ToEntity();
    protected override ProfileResponse MapToResponse(PersonalProfile entity) => entity.ToResponse();
    protected override void ApplyUpdate(PersonalProfile entity, UpdateProfileDto dto) => entity.ApplyUpdate(dto);

    public async Task<ProfileResponse?> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.FirstOrDefault()?.ToResponse();
    }
}

// ===== Education Service =====
public interface IEducationService : ICrudService<Education, CreateEducationDto, UpdateEducationDto, EducationResponse>
{
    Task<List<EducationResponse>> GetByUserIdAsync(int userId);
    Task<List<EducationResponse>> GetPublicByUserIdAsync(int userId);
}

public class EducationService : CrudService<Education, CreateEducationDto, UpdateEducationDto, EducationResponse>, IEducationService
{
    public EducationService(IRepository<Education> repo) : base(repo) { }
    protected override Education MapToEntity(CreateEducationDto dto) => dto.ToEntity();
    protected override EducationResponse MapToResponse(Education entity) => entity.ToResponse();
    protected override void ApplyUpdate(Education entity, UpdateEducationDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<EducationResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(e => e.UserId == userId);
        return items.OrderBy(e => e.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<EducationResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(e => e.UserId == userId && e.IsPublic);
        return items.OrderBy(e => e.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== WorkExperience Service =====
public interface IWorkExperienceService : ICrudService<WorkExperience, CreateWorkExperienceDto, UpdateWorkExperienceDto, WorkExperienceResponse>
{
    Task<List<WorkExperienceResponse>> GetByUserIdAsync(int userId);
    Task<List<WorkExperienceResponse>> GetPublicByUserIdAsync(int userId);
}

public class WorkExperienceService : CrudService<WorkExperience, CreateWorkExperienceDto, UpdateWorkExperienceDto, WorkExperienceResponse>, IWorkExperienceService
{
    public WorkExperienceService(IRepository<WorkExperience> repo) : base(repo) { }
    protected override WorkExperience MapToEntity(CreateWorkExperienceDto dto) => dto.ToEntity();
    protected override WorkExperienceResponse MapToResponse(WorkExperience entity) => entity.ToResponse();
    protected override void ApplyUpdate(WorkExperience entity, UpdateWorkExperienceDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<WorkExperienceResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId);
        return items.OrderBy(w => w.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<WorkExperienceResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.IsPublic);
        return items.OrderBy(w => w.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== Skill Service =====
public interface ISkillService : ICrudService<Skill, CreateSkillDto, UpdateSkillDto, SkillResponse>
{
    Task<List<SkillResponse>> GetByUserIdAsync(int userId);
    Task<List<SkillResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<SkillResponse>> GetByCategoryAsync(int userId, string category);
}

public class SkillService : CrudService<Skill, CreateSkillDto, UpdateSkillDto, SkillResponse>, ISkillService
{
    public SkillService(IRepository<Skill> repo) : base(repo) { }
    protected override Skill MapToEntity(CreateSkillDto dto) => dto.ToEntity();
    protected override SkillResponse MapToResponse(Skill entity) => entity.ToResponse();
    protected override void ApplyUpdate(Skill entity, UpdateSkillDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<SkillResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(s => s.UserId == userId);
        return items.OrderBy(s => s.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<SkillResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(s => s.UserId == userId && s.IsPublic);
        return items.OrderBy(s => s.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<SkillResponse>> GetByCategoryAsync(int userId, string category)
    {
        var items = await Repository.FindAsync(s => s.UserId == userId && s.Category == category);
        return items.OrderBy(s => s.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== Portfolio Service =====
public interface IPortfolioService : ICrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>
{
    Task<List<PortfolioResponse>> GetByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetFeaturedAsync(int userId);
}

public class PortfolioService : CrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>, IPortfolioService
{
    public PortfolioService(IRepository<Portfolio> repo) : base(repo) { }
    protected override Portfolio MapToEntity(CreatePortfolioDto dto) => dto.ToEntity();
    protected override PortfolioResponse MapToResponse(Portfolio entity) => entity.ToResponse();
    protected override void ApplyUpdate(Portfolio entity, UpdatePortfolioDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetFeaturedAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsFeatured && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== CalendarEvent Service =====
public interface ICalendarEventService : ICrudService<CalendarEvent, CreateCalendarEventDto, UpdateCalendarEventDto, CalendarEventResponse>
{
    Task<List<CalendarEventResponse>> GetByUserIdAsync(int userId);
    Task<List<CalendarEventResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<CalendarEventResponse>> GetByDateRangeAsync(int userId, DateTime start, DateTime end);
}

public class CalendarEventService : CrudService<CalendarEvent, CreateCalendarEventDto, UpdateCalendarEventDto, CalendarEventResponse>, ICalendarEventService
{
    public CalendarEventService(IRepository<CalendarEvent> repo) : base(repo) { }
    protected override CalendarEvent MapToEntity(CreateCalendarEventDto dto) => dto.ToEntity();
    protected override CalendarEventResponse MapToResponse(CalendarEvent entity) => entity.ToResponse();
    protected override void ApplyUpdate(CalendarEvent entity, UpdateCalendarEventDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<CalendarEventResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }

    public async Task<List<CalendarEventResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId && c.IsPublic);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }

    public async Task<List<CalendarEventResponse>> GetByDateRangeAsync(int userId, DateTime start, DateTime end)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId && c.StartTime >= start && c.EndTime <= end);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }
}

// ===== TodoItem Service =====
public interface ITodoItemService : ICrudService<TodoItem, CreateTodoItemDto, UpdateTodoItemDto, TodoItemResponse>
{
    Task<List<TodoItemResponse>> GetByUserIdAsync(int userId);
    Task<List<TodoItemResponse>> GetByStatusAsync(int userId, TodoStatus status);
}

public class TodoItemService : CrudService<TodoItem, CreateTodoItemDto, UpdateTodoItemDto, TodoItemResponse>, ITodoItemService
{
    public TodoItemService(IRepository<TodoItem> repo) : base(repo) { }
    protected override TodoItem MapToEntity(CreateTodoItemDto dto) => dto.ToEntity();
    protected override TodoItemResponse MapToResponse(TodoItem entity) => entity.ToResponse();
    protected override void ApplyUpdate(TodoItem entity, UpdateTodoItemDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<TodoItemResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId);
        return items.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).Select(MapToResponse).ToList();
    }

    public async Task<List<TodoItemResponse>> GetByStatusAsync(int userId, TodoStatus status)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId && t.Status == status);
        return items.OrderByDescending(t => t.Priority).Select(MapToResponse).ToList();
    }
}

// ===== WorkTask Service =====
public interface IWorkTaskService : ICrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>
{
    Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId);
    Task<List<WorkTaskResponse>> GetByProjectAsync(int userId, string project);
    Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status);
}

public class WorkTaskService : CrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>, IWorkTaskService
{
    public WorkTaskService(IRepository<WorkTask> repo) : base(repo) { }
    protected override WorkTask MapToEntity(CreateWorkTaskDto dto) => dto.ToEntity();
    protected override WorkTaskResponse MapToResponse(WorkTask entity) => entity.ToResponse();
    protected override void ApplyUpdate(WorkTask entity, UpdateWorkTaskDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId);
        return items.OrderByDescending(w => w.Priority).ThenBy(w => w.DueDate).Select(MapToResponse).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByProjectAsync(int userId, string project)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.Project == project);
        return items.OrderByDescending(w => w.Priority).Select(MapToResponse).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.Status == status);
        return items.OrderByDescending(w => w.Priority).Select(MapToResponse).ToList();
    }
}

// ===== BlogPost Service =====
public interface IBlogPostService : ICrudService<BlogPost, CreateBlogPostDto, UpdateBlogPostDto, BlogPostResponse>
{
    Task<List<BlogPostResponse>> GetByUserIdAsync(int userId);
    Task<List<BlogPostResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<BlogPostResponse>> GetPublishedAsync();
    Task<BlogPostResponse?> GetBySlugAsync(string slug);
}

public class BlogPostService : CrudService<BlogPost, CreateBlogPostDto, UpdateBlogPostDto, BlogPostResponse>, IBlogPostService
{
    public BlogPostService(IRepository<BlogPost> repo) : base(repo) { }

    protected override BlogPost MapToEntity(CreateBlogPostDto dto)
    {
        var entity = dto.ToEntity();
        entity.Slug = GenerateSlug(dto.Title);
        if (dto.Status == BlogPostStatus.Published)
            entity.PublishedAt = DateTime.UtcNow;
        return entity;
    }

    protected override BlogPostResponse MapToResponse(BlogPost entity) => entity.ToResponse();

    protected override void ApplyUpdate(BlogPost entity, UpdateBlogPostDto dto)
    {
        var wasDraft = entity.Status != BlogPostStatus.Published;
        entity.ApplyUpdate(dto);
        if (wasDraft && entity.Status == BlogPostStatus.Published)
            entity.PublishedAt = DateTime.UtcNow;
        if (dto.Title != null)
            entity.Slug = GenerateSlug(dto.Title);
    }

    public async Task<List<BlogPostResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(b => b.UserId == userId);
        return items.OrderByDescending(b => b.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<List<BlogPostResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(b => b.UserId == userId && b.Status == BlogPostStatus.Published && b.IsPublic);
        return items.OrderByDescending(b => b.PublishedAt).Select(MapToResponse).ToList();
    }

    public async Task<List<BlogPostResponse>> GetPublishedAsync()
    {
        var items = await Repository.FindAsync(b => b.Status == BlogPostStatus.Published && b.IsPublic);
        return items.OrderByDescending(b => b.PublishedAt).Select(MapToResponse).ToList();
    }

    public async Task<BlogPostResponse?> GetBySlugAsync(string slug)
    {
        var items = await Repository.FindAsync(b => b.Slug == slug);
        return items.FirstOrDefault()?.ToResponse();
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("　", "-");
        // Remove non-alphanumeric except hyphens and CJK chars
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^\w\u4e00-\u9fff\u3040-\u309f\u30a0-\u30ff-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }
}

// ===== GuestBookEntry Service =====
public interface IGuestBookEntryService : ICrudService<GuestBookEntry, CreateGuestBookEntryDto, UpdateGuestBookEntryDto, GuestBookEntryResponse>
{
    Task<List<GuestBookEntryResponse>> GetApprovedAsync();
    Task<List<GuestBookEntryResponse>> GetApprovedByTargetUserIdAsync(int targetUserId);
}

public class GuestBookEntryService : CrudService<GuestBookEntry, CreateGuestBookEntryDto, UpdateGuestBookEntryDto, GuestBookEntryResponse>, IGuestBookEntryService
{
    public GuestBookEntryService(IRepository<GuestBookEntry> repo) : base(repo) { }
    protected override GuestBookEntry MapToEntity(CreateGuestBookEntryDto dto) => dto.ToEntity();
    protected override GuestBookEntryResponse MapToResponse(GuestBookEntry entity) => entity.ToResponse();
    protected override void ApplyUpdate(GuestBookEntry entity, UpdateGuestBookEntryDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<GuestBookEntryResponse>> GetApprovedAsync()
    {
        var items = await Repository.FindAsync(g => g.IsApproved);
        return items.OrderByDescending(g => g.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<List<GuestBookEntryResponse>> GetApprovedByTargetUserIdAsync(int targetUserId)
    {
        var items = await Repository.FindAsync(g => g.TargetUserId == targetUserId && g.IsApproved);
        return items.OrderByDescending(g => g.CreatedAt).Select(MapToResponse).ToList();
    }
}

// ===== ContactMethod Service =====
public interface IContactMethodService : ICrudService<ContactMethod, CreateContactMethodDto, UpdateContactMethodDto, ContactMethodResponse>
{
    Task<List<ContactMethodResponse>> GetByUserIdAsync(int userId);
    Task<List<ContactMethodResponse>> GetPublicByUserIdAsync(int userId);
}

public class ContactMethodService : CrudService<ContactMethod, CreateContactMethodDto, UpdateContactMethodDto, ContactMethodResponse>, IContactMethodService
{
    public ContactMethodService(IRepository<ContactMethod> repo) : base(repo) { }
    protected override ContactMethod MapToEntity(CreateContactMethodDto dto) => dto.ToEntity();
    protected override ContactMethodResponse MapToResponse(ContactMethod entity) => entity.ToResponse();
    protected override void ApplyUpdate(ContactMethod entity, UpdateContactMethodDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<ContactMethodResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId);
        return items.OrderBy(c => c.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<ContactMethodResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId && c.IsPublic);
        return items.OrderBy(c => c.SortOrder).Select(MapToResponse).ToList();
    }
}
