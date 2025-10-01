using Microsoft.EntityFrameworkCore;
using PersonalManagerAPI.Data;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 資料庫連線測試服務
/// 用於測試 Entity Framework 連線和基本操作
/// </summary>
public class DatabaseConnectionTestService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseConnectionTestService> _logger;

    public DatabaseConnectionTestService(ApplicationDbContext context, ILogger<DatabaseConnectionTestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 測試資料庫連線
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing database connection...");
            
            // 嘗試連線到資料庫
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                _logger.LogInformation("Database connection successful");
                return true;
            }
            else
            {
                _logger.LogWarning("Cannot connect to database");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connection");
            return false;
        }
    }

    /// <summary>
    /// 檢查資料庫是否已建立
    /// </summary>
    public async Task<bool> IsDatabaseCreatedAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if database exists");
            return false;
        }
    }

    /// <summary>
    /// 建立資料庫（如果不存在）
    /// </summary>
    public async Task<bool> EnsureDatabaseCreatedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is created...");
            
            var created = await _context.Database.EnsureCreatedAsync();
            
            if (created)
            {
                _logger.LogInformation("Database created successfully");
            }
            else
            {
                _logger.LogInformation("Database already exists");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database");
            return false;
        }
    }

    /// <summary>
    /// 執行資料庫遷移
    /// </summary>
    public async Task<bool> MigrateDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Running database migrations...");
            
            await _context.Database.MigrateAsync();
            
            _logger.LogInformation("Database migrations completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running database migrations");
            return false;
        }
    }

    /// <summary>
    /// 檢查資料表是否存在
    /// </summary>
    public async Task<bool> CheckTablesExistAsync()
    {
        try
        {
            _logger.LogInformation("Checking if tables exist...");

            // 檢查主要資料表
            var userCount = await _context.Users.CountAsync();
            _logger.LogInformation("Users table exists with {Count} records", userCount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tables");
            return false;
        }
    }

    /// <summary>
    /// 執行基本 CRUD 測試
    /// </summary>
    public async Task<bool> TestBasicCrudAsync()
    {
        try
        {
            _logger.LogInformation("Testing basic CRUD operations...");

            // 測試建立
            var testUser = new User
            {
                Username = $"testuser_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Email = $"test_{DateTime.UtcNow:yyyyMMddHHmmss}@example.com",
                PasswordHash = "test_password_hash",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Create test passed - User ID: {UserId}", testUser.Id);

            // 測試讀取
            var retrievedUser = await _context.Users.FindAsync(testUser.Id);
            if (retrievedUser == null)
            {
                _logger.LogError("Read test failed - User not found");
                return false;
            }
            _logger.LogInformation("Read test passed - Retrieved user: {Username}", retrievedUser.Username);

            // 測試更新
            retrievedUser.Email = $"updated_{DateTime.UtcNow:yyyyMMddHHmmss}@example.com";
            retrievedUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Update test passed - Updated email: {Email}", retrievedUser.Email);

            // 測試刪除
            _context.Users.Remove(retrievedUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Delete test passed - User removed");

            _logger.LogInformation("All CRUD tests passed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CRUD tests");
            return false;
        }
    }

    /// <summary>
    /// 取得資料庫資訊
    /// </summary>
    public async Task<Dictionary<string, object>> GetDatabaseInfoAsync()
    {
        var info = new Dictionary<string, object>();

        try
        {
            // 取得資料庫提供者
            info["Provider"] = _context.Database.ProviderName ?? "Unknown";

            // 取得連線字串 (隱藏敏感資訊)
            var connectionString = _context.Database.GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                // 只顯示伺服器和資料庫名稱
                var parts = connectionString.Split(';');
                var serverPart = parts.FirstOrDefault(p => p.StartsWith("Server="));
                var databasePart = parts.FirstOrDefault(p => p.StartsWith("Database="));
                info["Server"] = serverPart?.Split('=')[1] ?? "Unknown";
                info["Database"] = databasePart?.Split('=')[1] ?? "Unknown";
            }

            // 取得待套用的遷移
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            info["PendingMigrations"] = pendingMigrations.ToList();

            // 取得已套用的遷移
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            info["AppliedMigrations"] = appliedMigrations.ToList();

            // 取得表格統計
            if (await _context.Database.CanConnectAsync())
            {
                info["UserCount"] = await _context.Users.CountAsync();
                info["PersonalProfileCount"] = await _context.PersonalProfiles.CountAsync();
                info["EducationCount"] = await _context.Educations.CountAsync();
                info["SkillCount"] = await _context.Skills.CountAsync();
                info["PortfolioCount"] = await _context.Portfolios.CountAsync();
                info["CalendarEventCount"] = await _context.CalendarEvents.CountAsync();
                info["TodoItemCount"] = await _context.TodoItems.CountAsync();
                info["WorkTaskCount"] = await _context.WorkTasks.CountAsync();
                info["BlogPostCount"] = await _context.BlogPosts.CountAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database info");
            info["Error"] = ex.Message;
        }

        return info;
    }
}