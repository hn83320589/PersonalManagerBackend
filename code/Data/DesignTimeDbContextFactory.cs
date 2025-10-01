using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalManagerAPI.Data;

/// <summary>
/// Design-time DbContext Factory for Entity Framework migrations
/// 用於 Entity Framework migrations 的設計時 DbContext Factory
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // 使用 MariaDB/MySQL 進行 migration 生成
        // 使用假的連線字串，只是為了讓 Entity Framework 工具知道使用 MySQL 語法
        var connectionString = "Server=localhost;Database=PersonalManagerDB;Uid=root;Pwd=password;";
        
        // 使用 MariaDB 8.0 版本來生成適當的語法
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}