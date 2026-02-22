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
