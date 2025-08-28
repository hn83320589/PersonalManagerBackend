using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// Simple User model for testing
public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Simple DbContext for testing
public class TestDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔧 Personal Manager - Entity Framework & MariaDB Integration Test");
        Console.WriteLine("================================================================");
        
        // Connection strings to test
        var connectionStrings = new[]
        {
            "Server=localhost;Database=personal_manager_test;Uid=root;Pwd=your_password;",
            "Server=localhost;Database=personal_manager_test;Uid=root;Pwd=;", // No password
            "Server=localhost;Database=personal_manager_test;Uid=root;Pwd=root;", // Password = root
        };

        bool testPassed = false;

        foreach (var connectionString in connectionStrings)
        {
            Console.WriteLine($"\n🔍 Testing connection: Server=localhost;Database=personal_manager_test;Uid=root;Pwd=***");
            
            try
            {
                var options = new DbContextOptionsBuilder<TestDbContext>()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .EnableSensitiveDataLogging()
                    .Options;

                using var context = new TestDbContext(options);
                
                // Test 1: Database connection
                Console.WriteLine("📡 Testing database connection...");
                await context.Database.CanConnectAsync();
                Console.WriteLine("✅ Database connection successful!");

                // Test 2: Create database and tables
                Console.WriteLine("🏗️ Creating database and tables...");
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Database and tables created successfully!");

                // Test 3: Insert test data
                Console.WriteLine("💾 Testing data insertion...");
                var testUser = new User
                {
                    Username = "testuser" + DateTime.Now.Ticks,
                    Email = $"test{DateTime.Now.Ticks}@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                    FirstName = "Test",
                    LastName = "User"
                };

                context.Users.Add(testUser);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Test user created with ID: {testUser.Id}");

                // Test 4: Query data
                Console.WriteLine("🔍 Testing data query...");
                var userCount = await context.Users.CountAsync();
                var users = await context.Users.Take(5).ToListAsync();
                Console.WriteLine($"✅ Total users in database: {userCount}");
                Console.WriteLine($"✅ Retrieved {users.Count} users successfully");

                // Test 5: Update data
                Console.WriteLine("✏️ Testing data update...");
                testUser.FirstName = "Updated Test";
                await context.SaveChangesAsync();
                Console.WriteLine("✅ User data updated successfully");

                // Test 6: Delete data
                Console.WriteLine("🗑️ Testing data deletion...");
                context.Users.Remove(testUser);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ User data deleted successfully");

                testPassed = true;
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
                }
                continue;
            }
        }

        if (testPassed)
        {
            Console.WriteLine("\n🎉 All tests passed! Entity Framework integration with MariaDB is working correctly.");
            Console.WriteLine("✅ Ready to proceed with Phase 2.1 database integration.");
            Console.WriteLine("\nNext steps:");
            Console.WriteLine("1. Create Entity Framework Migrations in main project");
            Console.WriteLine("2. Update service layer to use EF DbContext instead of JsonDataService");
            Console.WriteLine("3. Run comprehensive integration tests");
        }
        else
        {
            Console.WriteLine("\n❌ All connection attempts failed. Please ensure:");
            Console.WriteLine("   - MariaDB server is running");
            Console.WriteLine("   - MySQL/MariaDB is installed and accessible");
            Console.WriteLine("   - Check username/password combinations");
            Console.WriteLine("   - Verify server is listening on localhost:3306");
            Console.WriteLine("\nTo install MariaDB:");
            Console.WriteLine("   - Download from: https://mariadb.org/download/");
            Console.WriteLine("   - Or use Docker: docker run --name mariadb -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 -d mariadb:latest");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
