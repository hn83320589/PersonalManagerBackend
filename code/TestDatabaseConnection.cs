using Microsoft.EntityFrameworkCore;
using PersonalManagerAPI.Data;

namespace PersonalManagerAPI
{
    /// <summary>
    /// Simple database connection test utility
    /// </summary>
    public class DatabaseConnectionTest
    {
        public static async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .Options;

                using var context = new ApplicationDbContext(options);
                
                // Test database connection
                await context.Database.CanConnectAsync();
                
                Console.WriteLine("✅ Database connection successful!");
                Console.WriteLine($"Connection string: {connectionString}");
                
                // Check if database exists
                var exists = await context.Database.CanConnectAsync();
                Console.WriteLine($"✅ Database exists: {exists}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                return false;
            }
        }
        
        public static async Task<bool> TestCreateTablesAsync(string connectionString)
        {
            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .Options;

                using var context = new ApplicationDbContext(options);
                
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
                
                Console.WriteLine("✅ Database and tables created successfully!");
                
                // Test basic CRUD operations
                var testUser = new PersonalManagerAPI.Models.User
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                    FirstName = "Test",
                    LastName = "User",
                    Role = "User"
                };
                
                context.Users.Add(testUser);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✅ Test user created successfully!");
                
                // Test query
                var users = await context.Users.CountAsync();
                Console.WriteLine($"✅ Total users in database: {users}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database operation failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}