using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PersonalProfile> PersonalProfiles => Set<PersonalProfile>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<GuestBookEntry> GuestBookEntries => Set<GuestBookEntry>();
    public DbSet<ContactMethod> ContactMethods => Set<ContactMethod>();
    public DbSet<FileUpload> FileUploads => Set<FileUpload>();
    public DbSet<PortfolioAttachment> PortfolioAttachments => Set<PortfolioAttachment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enum → string conversions (store as readable strings in DB)
        modelBuilder.Entity<Skill>()
            .Property(e => e.Level)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<BlogPost>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<TodoItem>()
            .Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<TodoItem>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<ContactMethod>()
            .Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        // WorkTask → Project FK (nullable, set null on delete)
        modelBuilder.Entity<WorkTask>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // BlogPost ↔ Tag many-to-many
        modelBuilder.Entity<BlogPost>()
            .HasMany(b => b.TagEntities)
            .WithMany()
            .UsingEntity("BlogPostTags");

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();
    }
}
