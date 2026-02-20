using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.DTOs;

// ===== User =====
public class CreateUserDto
{
    [Required, StringLength(50)] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== PersonalProfile =====
public class CreateProfileDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
public class UpdateProfileDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
}
public class ProfileResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== Education =====
public class CreateEducationDto
{
    public int UserId { get; set; }
    [Required] public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdateEducationDto
{
    public string? School { get; set; }
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class EducationResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
}

// ===== WorkExperience =====
public class CreateWorkExperienceDto
{
    public int UserId { get; set; }
    [Required] public string Company { get; set; } = string.Empty;
    [Required] public string Position { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdateWorkExperienceDto
{
    public string? Company { get; set; }
    public string? Position { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class WorkExperienceResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
}

// ===== Skill =====
public class CreateSkillDto
{
    public int UserId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
    public int YearsOfExperience { get; set; }
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdateSkillDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public SkillLevel? Level { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class SkillResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public SkillLevel Level { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
}

// ===== Portfolio =====
public class CreatePortfolioDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdatePortfolioDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? Technologies { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class PortfolioResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== CalendarEvent =====
public class CreateCalendarEventDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsPublic { get; set; }
    public string Color { get; set; } = string.Empty;
}
public class UpdateCalendarEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsAllDay { get; set; }
    public bool? IsPublic { get; set; }
    public string? Color { get; set; }
}
public class CalendarEventResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsPublic { get; set; }
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ===== TodoItem =====
public class CreateTodoItemDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    public DateTime? DueDate { get; set; }
}
public class UpdateTodoItemDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TodoPriority? Priority { get; set; }
    public TodoStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
}
public class TodoItemResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoPriority Priority { get; set; }
    public TodoStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== WorkTask =====
public class CreateWorkTaskDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public WorkTaskPriority Priority { get; set; } = WorkTaskPriority.Medium;
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;
    public double EstimatedHours { get; set; }
    public DateTime? DueDate { get; set; }
    public string Tags { get; set; } = string.Empty;
}
public class UpdateWorkTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Project { get; set; }
    public WorkTaskPriority? Priority { get; set; }
    public WorkTaskStatus? Status { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Tags { get; set; }
}
public class WorkTaskResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public WorkTaskPriority Priority { get; set; }
    public WorkTaskStatus Status { get; set; }
    public double EstimatedHours { get; set; }
    public double ActualHours { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Tags { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== BlogPost =====
public class CreateBlogPostDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
    public bool IsPublic { get; set; } = true;
}
public class UpdateBlogPostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public BlogPostStatus? Status { get; set; }
    public bool? IsPublic { get; set; }
}
public class BlogPostResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public BlogPostStatus Status { get; set; }
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== GuestBookEntry =====
public class CreateGuestBookEntryDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;
}
public class UpdateGuestBookEntryDto
{
    public bool? IsApproved { get; set; }
    public string? AdminReply { get; set; }
}
public class GuestBookEntryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string AdminReply { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ===== ContactMethod =====
public class CreateContactMethodDto
{
    public int UserId { get; set; }
    public ContactType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    [Required] public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdateContactMethodDto
{
    public ContactType? Type { get; set; }
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string? Icon { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class ContactMethodResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ContactType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
}
