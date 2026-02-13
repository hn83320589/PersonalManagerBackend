using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Mappings;

public static class MappingExtensions
{
    // ===== User =====
    public static UserResponse ToResponse(this User u) => new()
    {
        Id = u.Id, Username = u.Username, Email = u.Email,
        FullName = u.FullName, Role = u.Role, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
    public static User ToEntity(this CreateUserDto d) => new()
    {
        Username = d.Username, Email = d.Email, FullName = d.FullName, Role = d.Role
    };
    public static void ApplyUpdate(this User u, UpdateUserDto d)
    {
        if (d.Email != null) u.Email = d.Email;
        if (d.FullName != null) u.FullName = d.FullName;
        if (d.Role != null) u.Role = d.Role;
        if (d.IsActive.HasValue) u.IsActive = d.IsActive.Value;
    }

    // ===== PersonalProfile =====
    public static ProfileResponse ToResponse(this PersonalProfile p) => new()
    {
        Id = p.Id, UserId = p.UserId, Title = p.Title, Summary = p.Summary,
        Description = p.Description, ProfileImageUrl = p.ProfileImageUrl,
        Website = p.Website, Location = p.Location, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
    public static PersonalProfile ToEntity(this CreateProfileDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Summary = d.Summary, Description = d.Description,
        ProfileImageUrl = d.ProfileImageUrl, Website = d.Website, Location = d.Location
    };
    public static void ApplyUpdate(this PersonalProfile p, UpdateProfileDto d)
    {
        if (d.Title != null) p.Title = d.Title;
        if (d.Summary != null) p.Summary = d.Summary;
        if (d.Description != null) p.Description = d.Description;
        if (d.ProfileImageUrl != null) p.ProfileImageUrl = d.ProfileImageUrl;
        if (d.Website != null) p.Website = d.Website;
        if (d.Location != null) p.Location = d.Location;
    }

    // ===== Education =====
    public static EducationResponse ToResponse(this Education e) => new()
    {
        Id = e.Id, UserId = e.UserId, School = e.School, Degree = e.Degree,
        FieldOfStudy = e.FieldOfStudy, StartYear = e.StartYear, EndYear = e.EndYear,
        Description = e.Description, IsPublic = e.IsPublic, SortOrder = e.SortOrder
    };
    public static Education ToEntity(this CreateEducationDto d) => new()
    {
        UserId = d.UserId, School = d.School, Degree = d.Degree, FieldOfStudy = d.FieldOfStudy,
        StartYear = d.StartYear, EndYear = d.EndYear, Description = d.Description,
        IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Education e, UpdateEducationDto d)
    {
        if (d.School != null) e.School = d.School;
        if (d.Degree != null) e.Degree = d.Degree;
        if (d.FieldOfStudy != null) e.FieldOfStudy = d.FieldOfStudy;
        if (d.StartYear.HasValue) e.StartYear = d.StartYear;
        if (d.EndYear.HasValue) e.EndYear = d.EndYear;
        if (d.Description != null) e.Description = d.Description;
        if (d.IsPublic.HasValue) e.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) e.SortOrder = d.SortOrder.Value;
    }

    // ===== WorkExperience =====
    public static WorkExperienceResponse ToResponse(this WorkExperience w) => new()
    {
        Id = w.Id, UserId = w.UserId, Company = w.Company, Position = w.Position,
        StartDate = w.StartDate, EndDate = w.EndDate, IsCurrent = w.IsCurrent,
        Description = w.Description, IsPublic = w.IsPublic, SortOrder = w.SortOrder
    };
    public static WorkExperience ToEntity(this CreateWorkExperienceDto d) => new()
    {
        UserId = d.UserId, Company = d.Company, Position = d.Position,
        StartDate = d.StartDate, EndDate = d.EndDate, IsCurrent = d.IsCurrent,
        Description = d.Description, IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this WorkExperience w, UpdateWorkExperienceDto d)
    {
        if (d.Company != null) w.Company = d.Company;
        if (d.Position != null) w.Position = d.Position;
        if (d.StartDate.HasValue) w.StartDate = d.StartDate;
        if (d.EndDate.HasValue) w.EndDate = d.EndDate;
        if (d.IsCurrent.HasValue) w.IsCurrent = d.IsCurrent.Value;
        if (d.Description != null) w.Description = d.Description;
        if (d.IsPublic.HasValue) w.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) w.SortOrder = d.SortOrder.Value;
    }

    // ===== Skill =====
    public static SkillResponse ToResponse(this Skill s) => new()
    {
        Id = s.Id, UserId = s.UserId, Name = s.Name, Category = s.Category,
        Level = s.Level, YearsOfExperience = s.YearsOfExperience,
        IsPublic = s.IsPublic, SortOrder = s.SortOrder
    };
    public static Skill ToEntity(this CreateSkillDto d) => new()
    {
        UserId = d.UserId, Name = d.Name, Category = d.Category,
        Level = d.Level, YearsOfExperience = d.YearsOfExperience,
        IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Skill s, UpdateSkillDto d)
    {
        if (d.Name != null) s.Name = d.Name;
        if (d.Category != null) s.Category = d.Category;
        if (d.Level.HasValue) s.Level = d.Level.Value;
        if (d.YearsOfExperience.HasValue) s.YearsOfExperience = d.YearsOfExperience.Value;
        if (d.IsPublic.HasValue) s.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) s.SortOrder = d.SortOrder.Value;
    }

    // ===== Portfolio =====
    public static PortfolioResponse ToResponse(this Portfolio p) => new()
    {
        Id = p.Id, UserId = p.UserId, Title = p.Title, Description = p.Description,
        ImageUrl = p.ImageUrl, ProjectUrl = p.ProjectUrl, RepositoryUrl = p.RepositoryUrl,
        Technologies = p.Technologies, IsFeatured = p.IsFeatured, IsPublic = p.IsPublic,
        SortOrder = p.SortOrder, CreatedAt = p.CreatedAt
    };
    public static Portfolio ToEntity(this CreatePortfolioDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        ImageUrl = d.ImageUrl, ProjectUrl = d.ProjectUrl, RepositoryUrl = d.RepositoryUrl,
        Technologies = d.Technologies, IsFeatured = d.IsFeatured,
        IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Portfolio p, UpdatePortfolioDto d)
    {
        if (d.Title != null) p.Title = d.Title;
        if (d.Description != null) p.Description = d.Description;
        if (d.ImageUrl != null) p.ImageUrl = d.ImageUrl;
        if (d.ProjectUrl != null) p.ProjectUrl = d.ProjectUrl;
        if (d.RepositoryUrl != null) p.RepositoryUrl = d.RepositoryUrl;
        if (d.Technologies != null) p.Technologies = d.Technologies;
        if (d.IsFeatured.HasValue) p.IsFeatured = d.IsFeatured.Value;
        if (d.IsPublic.HasValue) p.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) p.SortOrder = d.SortOrder.Value;
    }

    // ===== CalendarEvent =====
    public static CalendarEventResponse ToResponse(this CalendarEvent c) => new()
    {
        Id = c.Id, UserId = c.UserId, Title = c.Title, Description = c.Description,
        StartTime = c.StartTime, EndTime = c.EndTime, IsAllDay = c.IsAllDay,
        IsPublic = c.IsPublic, Color = c.Color, CreatedAt = c.CreatedAt
    };
    public static CalendarEvent ToEntity(this CreateCalendarEventDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        StartTime = d.StartTime, EndTime = d.EndTime, IsAllDay = d.IsAllDay,
        IsPublic = d.IsPublic, Color = d.Color
    };
    public static void ApplyUpdate(this CalendarEvent c, UpdateCalendarEventDto d)
    {
        if (d.Title != null) c.Title = d.Title;
        if (d.Description != null) c.Description = d.Description;
        if (d.StartTime.HasValue) c.StartTime = d.StartTime.Value;
        if (d.EndTime.HasValue) c.EndTime = d.EndTime.Value;
        if (d.IsAllDay.HasValue) c.IsAllDay = d.IsAllDay.Value;
        if (d.IsPublic.HasValue) c.IsPublic = d.IsPublic.Value;
        if (d.Color != null) c.Color = d.Color;
    }

    // ===== TodoItem =====
    public static TodoItemResponse ToResponse(this TodoItem t) => new()
    {
        Id = t.Id, UserId = t.UserId, Title = t.Title, Description = t.Description,
        Priority = t.Priority, Status = t.Status, DueDate = t.DueDate,
        CompletedAt = t.CompletedAt, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
    };
    public static TodoItem ToEntity(this CreateTodoItemDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        Priority = d.Priority, Status = d.Status, DueDate = d.DueDate
    };
    public static void ApplyUpdate(this TodoItem t, UpdateTodoItemDto d)
    {
        if (d.Title != null) t.Title = d.Title;
        if (d.Description != null) t.Description = d.Description;
        if (d.Priority.HasValue) t.Priority = d.Priority.Value;
        if (d.Status.HasValue) t.Status = d.Status.Value;
        if (d.DueDate.HasValue) t.DueDate = d.DueDate;
        if (d.CompletedAt.HasValue) t.CompletedAt = d.CompletedAt;
    }

    // ===== WorkTask =====
    public static WorkTaskResponse ToResponse(this WorkTask w) => new()
    {
        Id = w.Id, UserId = w.UserId, Title = w.Title, Description = w.Description,
        Project = w.Project, Priority = w.Priority, Status = w.Status,
        EstimatedHours = w.EstimatedHours, ActualHours = w.ActualHours,
        DueDate = w.DueDate, CompletedAt = w.CompletedAt, Tags = w.Tags,
        CreatedAt = w.CreatedAt, UpdatedAt = w.UpdatedAt
    };
    public static WorkTask ToEntity(this CreateWorkTaskDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        Project = d.Project, Priority = d.Priority, Status = d.Status,
        EstimatedHours = d.EstimatedHours, DueDate = d.DueDate, Tags = d.Tags
    };
    public static void ApplyUpdate(this WorkTask w, UpdateWorkTaskDto d)
    {
        if (d.Title != null) w.Title = d.Title;
        if (d.Description != null) w.Description = d.Description;
        if (d.Project != null) w.Project = d.Project;
        if (d.Priority.HasValue) w.Priority = d.Priority.Value;
        if (d.Status.HasValue) w.Status = d.Status.Value;
        if (d.EstimatedHours.HasValue) w.EstimatedHours = d.EstimatedHours.Value;
        if (d.ActualHours.HasValue) w.ActualHours = d.ActualHours.Value;
        if (d.DueDate.HasValue) w.DueDate = d.DueDate;
        if (d.CompletedAt.HasValue) w.CompletedAt = d.CompletedAt;
        if (d.Tags != null) w.Tags = d.Tags;
    }

    // ===== BlogPost =====
    public static BlogPostResponse ToResponse(this BlogPost b) => new()
    {
        Id = b.Id, UserId = b.UserId, Title = b.Title, Slug = b.Slug,
        Content = b.Content, Summary = b.Summary, Category = b.Category,
        Tags = b.Tags, Status = b.Status, IsPublic = b.IsPublic,
        ViewCount = b.ViewCount, PublishedAt = b.PublishedAt,
        CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt
    };
    public static BlogPost ToEntity(this CreateBlogPostDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Content = d.Content,
        Summary = d.Summary, Category = d.Category, Tags = d.Tags,
        Status = d.Status, IsPublic = d.IsPublic
    };
    public static void ApplyUpdate(this BlogPost b, UpdateBlogPostDto d)
    {
        if (d.Title != null) b.Title = d.Title;
        if (d.Content != null) b.Content = d.Content;
        if (d.Summary != null) b.Summary = d.Summary;
        if (d.Category != null) b.Category = d.Category;
        if (d.Tags != null) b.Tags = d.Tags;
        if (d.Status.HasValue) b.Status = d.Status.Value;
        if (d.IsPublic.HasValue) b.IsPublic = d.IsPublic.Value;
    }

    // ===== GuestBookEntry =====
    public static GuestBookEntryResponse ToResponse(this GuestBookEntry g) => new()
    {
        Id = g.Id, Name = g.Name, Email = g.Email, Message = g.Message,
        IsApproved = g.IsApproved, AdminReply = g.AdminReply, CreatedAt = g.CreatedAt
    };
    public static GuestBookEntry ToEntity(this CreateGuestBookEntryDto d) => new()
    {
        Name = d.Name, Email = d.Email, Message = d.Message
    };
    public static void ApplyUpdate(this GuestBookEntry g, UpdateGuestBookEntryDto d)
    {
        if (d.IsApproved.HasValue) g.IsApproved = d.IsApproved.Value;
        if (d.AdminReply != null) g.AdminReply = d.AdminReply;
    }

    // ===== ContactMethod =====
    public static ContactMethodResponse ToResponse(this ContactMethod c) => new()
    {
        Id = c.Id, UserId = c.UserId, Type = c.Type, Label = c.Label,
        Value = c.Value, Icon = c.Icon, IsPublic = c.IsPublic, SortOrder = c.SortOrder
    };
    public static ContactMethod ToEntity(this CreateContactMethodDto d) => new()
    {
        UserId = d.UserId, Type = d.Type, Label = d.Label, Value = d.Value,
        Icon = d.Icon, IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this ContactMethod c, UpdateContactMethodDto d)
    {
        if (d.Type.HasValue) c.Type = d.Type.Value;
        if (d.Label != null) c.Label = d.Label;
        if (d.Value != null) c.Value = d.Value;
        if (d.Icon != null) c.Icon = d.Icon;
        if (d.IsPublic.HasValue) c.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) c.SortOrder = d.SortOrder.Value;
    }
}
