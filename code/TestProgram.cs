using PersonalManagerAPI;

/// <summary>
/// Simple console program to test Entity Framework and MariaDB integration
/// </summary>
class TestProgram
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔧 Personal Manager - Entity Framework & MariaDB Integration Test");
        Console.WriteLine("================================================================");
        
        // Connection string for local MariaDB
        var connectionString = "Server=localhost;Database=personal_manager;Uid=root;Pwd=your_password;";
        
        Console.WriteLine("📡 Testing database connection...");
        var connectionSuccess = await DatabaseConnectionTest.TestConnectionAsync(connectionString);
        
        if (connectionSuccess)
        {
            Console.WriteLine("\n🏗️ Testing database creation and basic operations...");
            var operationSuccess = await DatabaseConnectionTest.TestCreateTablesAsync(connectionString);
            
            if (operationSuccess)
            {
                Console.WriteLine("\n🎉 All tests passed! Entity Framework integration with MariaDB is working correctly.");
                Console.WriteLine("✅ Ready to proceed with Phase 2.1 database integration.");
            }
            else
            {
                Console.WriteLine("\n❌ Database operations failed. Check MariaDB server and permissions.");
            }
        }
        else
        {
            Console.WriteLine("\n❌ Cannot connect to database. Please ensure:");
            Console.WriteLine("   - MariaDB server is running");
            Console.WriteLine("   - Database 'personal_manager' exists");
            Console.WriteLine("   - Username/password are correct");
            Console.WriteLine("   - Connection string is properly configured");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}