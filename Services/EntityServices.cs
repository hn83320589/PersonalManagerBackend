using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Mappings;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Settings;

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

// ===== Project Service =====
public interface IProjectService : ICrudService<Project, CreateProjectDto, UpdateProjectDto, ProjectResponse>
{
    Task<List<ProjectResponse>> GetByUserIdAsync(int userId);
}

public class ProjectService : CrudService<Project, CreateProjectDto, UpdateProjectDto, ProjectResponse>, IProjectService
{
    public ProjectService(IRepository<Project> repo) : base(repo) { }
    protected override Project MapToEntity(CreateProjectDto dto) => dto.ToEntity();
    protected override ProjectResponse MapToResponse(Project entity) => entity.ToResponse();
    protected override void ApplyUpdate(Project entity, UpdateProjectDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<ProjectResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).Select(MapToResponse).ToList();
    }
}

// ===== WorkTask Service =====
public interface IWorkTaskService : ICrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>
{
    Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId);
    Task<List<WorkTaskResponse>> GetByProjectIdAsync(int userId, int? projectId);
    Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status);
}

public class WorkTaskService : CrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>, IWorkTaskService
{
    private readonly IRepository<Project> _projectRepo;

    public WorkTaskService(IRepository<WorkTask> repo, IRepository<Project> projectRepo) : base(repo)
    {
        _projectRepo = projectRepo;
    }

    protected override WorkTask MapToEntity(CreateWorkTaskDto dto) => dto.ToEntity();
    protected override WorkTaskResponse MapToResponse(WorkTask entity) => entity.ToResponse();
    protected override void ApplyUpdate(WorkTask entity, UpdateWorkTaskDto dto) => entity.ApplyUpdate(dto);

    private async Task<Dictionary<int, string>> GetProjectMapAsync(IEnumerable<WorkTask> tasks)
    {
        var projectIds = tasks.Where(t => t.ProjectId.HasValue).Select(t => t.ProjectId!.Value).Distinct().ToList();
        if (projectIds.Count == 0) return new();
        var projects = await _projectRepo.FindAsync(p => projectIds.Contains(p.Id));
        return projects.ToDictionary(p => p.Id, p => p.Name);
    }

    public async Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority).ThenBy(w => w.DueDate)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByProjectIdAsync(int userId, int? projectId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.ProjectId == projectId);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.Status == status);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }
}

// ===== BlogPost Service =====
public interface IBlogPostService : ICrudService<BlogPost, CreateBlogPostDto, UpdateBlogPostDto, BlogPostResponse>
{
    Task<List<BlogPostResponse>> GetByUserIdAsync(int userId);
    Task<List<BlogPostResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<BlogPostResponse>> GetPublishedAsync();
    Task<BlogPostResponse?> GetBySlugAsync(string slug);
    Task IncrementViewCountAsync(int id);
    Task<PagedResult<BlogPostResponse>> GetPublicPagedAsync(int userId, int page, int pageSize, string? keyword, string? tag, string? category);
}

public class BlogPostService : CrudService<BlogPost, CreateBlogPostDto, UpdateBlogPostDto, BlogPostResponse>, IBlogPostService
{
    private readonly BlogPostRepository? _blogPostRepo;

    public BlogPostService(IRepository<BlogPost> repo) : base(repo)
    {
        _blogPostRepo = repo as BlogPostRepository;
    }

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

    public override async Task<BlogPostResponse> CreateAsync(CreateBlogPostDto dto)
    {
        var entity = MapToEntity(dto);
        entity = await Repository.AddAsync(entity);
        if (_blogPostRepo != null && dto.Tags.Count > 0)
            await _blogPostRepo.SyncTagsAsync(entity, dto.Tags);
        var loaded = await Repository.GetByIdAsync(entity.Id);
        return MapToResponse(loaded ?? entity);
    }

    public override async Task<BlogPostResponse?> UpdateAsync(int id, UpdateBlogPostDto dto)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return default;
        ApplyUpdate(entity, dto);
        entity = await Repository.UpdateAsync(entity);
        if (_blogPostRepo != null && dto.Tags != null)
            await _blogPostRepo.SyncTagsAsync(entity, dto.Tags);
        var loaded = await Repository.GetByIdAsync(entity.Id);
        return MapToResponse(loaded ?? entity);
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

    public async Task IncrementViewCountAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return;
        entity.ViewCount++;
        await Repository.UpdateAsync(entity);
    }

    public async Task<PagedResult<BlogPostResponse>> GetPublicPagedAsync(int userId, int page, int pageSize, string? keyword, string? tag, string? category)
    {
        var items = await Repository.FindAsync(b => b.UserId == userId && b.Status == BlogPostStatus.Published && b.IsPublic);
        IEnumerable<BlogPost> query = items.OrderByDescending(b => b.PublishedAt);

        if (!string.IsNullOrEmpty(keyword))
        {
            var kw = keyword.ToLowerInvariant();
            query = query.Where(b =>
                b.Title.ToLowerInvariant().Contains(kw) ||
                (b.Content?.ToLowerInvariant().Contains(kw) ?? false) ||
                (b.Summary?.ToLowerInvariant().Contains(kw) ?? false) ||
                b.TagEntities.Any(t => t.Name.ToLowerInvariant().Contains(kw)) ||
                (!b.TagEntities.Any() && b.Tags?.ToLowerInvariant().Contains(kw) == true));
        }
        if (!string.IsNullOrEmpty(tag))
            query = query.Where(b =>
                b.TagEntities.Any(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase)) ||
                (!b.TagEntities.Any() && b.Tags != null && b.Tags.Split(',').Select(t => t.Trim()).Contains(tag, StringComparer.OrdinalIgnoreCase)));
        if (!string.IsNullOrEmpty(category))
            query = query.Where(b => b.Category != null && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var filtered = query.ToList();
        return new PagedResult<BlogPostResponse>
        {
            Items = filtered.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList(),
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        };
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

// ===== FileUpload Service =====
public interface IFileUploadService
{
    Task<List<FileUploadResponse>> GetByUserIdAsync(int userId);
    Task<FileUploadResponse> UploadAsync(IFormFile file, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}

public class FileUploadService : IFileUploadService
{
    private readonly IRepository<FileUpload> _repo;
    private readonly IFileStorageProvider _storage;

    public FileUploadService(IRepository<FileUpload> repo, IFileStorageProvider storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<List<FileUploadResponse>> GetByUserIdAsync(int userId)
    {
        var items = await _repo.FindAsync(f => f.UserId == userId);
        return items.OrderByDescending(f => f.CreatedAt).Select(f => f.ToResponse()).ToList();
    }

    public async Task<FileUploadResponse> UploadAsync(IFormFile file, int userId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var (fileUrl, storedName) = await _storage.UploadAsync(file, ext);

        var fileType = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
            ".pdf" => "pdf",
            ".doc" or ".docx" => "document",
            ".ppt" or ".pptx" => "presentation",
            _ => "other"
        };

        var entity = new FileUpload
        {
            UserId = userId,
            FileName = file.FileName,
            StoredName = storedName,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(entity);
        return saved.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var items = await _repo.FindAsync(f => f.Id == id && f.UserId == userId);
        var entity = items.FirstOrDefault();
        if (entity == null) return false;

        await _storage.DeleteAsync(entity.StoredName);
        return await _repo.DeleteAsync(id);
    }
}

// ===== PortfolioAttachment Service =====
public interface IPortfolioAttachmentService : ICrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>
{
    Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId);
}

public class PortfolioAttachmentService : CrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>, IPortfolioAttachmentService
{
    public PortfolioAttachmentService(IRepository<PortfolioAttachment> repo) : base(repo) { }
    protected override PortfolioAttachment MapToEntity(CreatePortfolioAttachmentDto dto) => dto.ToEntity();
    protected override PortfolioAttachmentResponse MapToResponse(PortfolioAttachment entity) => entity.ToResponse();
    protected override void ApplyUpdate(PortfolioAttachment entity, UpdatePortfolioAttachmentDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId)
    {
        var items = await Repository.FindAsync(a => a.PortfolioId == portfolioId);
        return items.OrderBy(a => a.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== GuestBookEntry Service =====
public interface IGuestBookEntryService : ICrudService<GuestBookEntry, CreateGuestBookEntryDto, UpdateGuestBookEntryDto, GuestBookEntryResponse>
{
    Task<List<GuestBookEntryResponse>> GetApprovedAsync();
    Task<List<GuestBookEntryResponse>> GetApprovedByTargetUserIdAsync(int targetUserId);
    Task<PagedResult<GuestBookEntryResponse>> GetApprovedPagedAsync(int targetUserId, int page, int pageSize);
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

    public async Task<PagedResult<GuestBookEntryResponse>> GetApprovedPagedAsync(int targetUserId, int page, int pageSize)
    {
        var items = await Repository.FindAsync(g => g.TargetUserId == targetUserId && g.IsApproved);
        var ordered = items.OrderByDescending(g => g.CreatedAt).ToList();
        return new PagedResult<GuestBookEntryResponse>
        {
            Items = ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList(),
            TotalCount = ordered.Count,
            Page = page,
            PageSize = pageSize
        };
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

// ===== TimeEntry Service =====
public interface ITimeEntryService : ICrudService<TimeEntry, CreateTimeEntryDto, UpdateTimeEntryDto, TimeEntryResponse>
{
    Task<List<TimeEntryResponse>> GetByUserIdAsync(int userId);
}

public class TimeEntryService : CrudService<TimeEntry, CreateTimeEntryDto, UpdateTimeEntryDto, TimeEntryResponse>, ITimeEntryService
{
    public TimeEntryService(IRepository<TimeEntry> repo) : base(repo) { }
    protected override TimeEntry MapToEntity(CreateTimeEntryDto dto) => dto.ToEntity();
    protected override TimeEntryResponse MapToResponse(TimeEntry entity) => entity.ToResponse();
    protected override void ApplyUpdate(TimeEntry entity, UpdateTimeEntryDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<TimeEntryResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId);
        return items.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt).Select(MapToResponse).ToList();
    }
}
