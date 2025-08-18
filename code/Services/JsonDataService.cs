using System.Text.Json;
using System.Text.Json.Serialization;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services;

public class JsonDataService
{
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonDataService(IWebHostEnvironment environment)
    {
        _dataPath = Path.Combine(environment.ContentRootPath, "Data", "JsonData");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Use PascalCase to match C# properties
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    // Generic methods for reading and writing JSON data
    public async Task<List<T>> ReadJsonAsync<T>(string fileName)
    {
        var filePath = Path.Combine(_dataPath, fileName);
        
        if (!File.Exists(filePath))
        {
            return new List<T>();
        }

        var jsonContent = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<List<T>>(jsonContent, _jsonOptions);
        
        return data ?? new List<T>();
    }

    public async Task WriteJsonAsync<T>(string fileName, List<T> data)
    {
        var filePath = Path.Combine(_dataPath, fileName);
        
        // Ensure directory exists
        Directory.CreateDirectory(_dataPath);
        
        var jsonContent = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(filePath, jsonContent);
    }

    // Generic CRUD methods for any entity type
    public async Task<List<T>> GetAllAsync<T>()
    {
        var fileName = GetFileNameForType<T>();
        return await ReadJsonAsync<T>(fileName);
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : class
    {
        var items = await GetAllAsync<T>();
        return items.FirstOrDefault(item => GetId(item) == id);
    }

    public async Task<T> CreateAsync<T>(T item) where T : class
    {
        var items = await GetAllAsync<T>();
        SetId(item, items.Count > 0 ? items.Max(x => GetId(x)) + 1 : 1);
        items.Add(item);
        await SaveAllAsync(items);
        return item;
    }

    public async Task<T?> UpdateAsync<T>(T item) where T : class
    {
        var items = await GetAllAsync<T>();
        var index = items.FindIndex(x => GetId(x) == GetId(item));
        if (index >= 0)
        {
            items[index] = item;
            await SaveAllAsync(items);
            return item;
        }
        return null;
    }

    public async Task<bool> DeleteAsync<T>(int id) where T : class
    {
        var items = await GetAllAsync<T>();
        var item = items.FirstOrDefault(x => GetId(x) == id);
        if (item != null)
        {
            items.Remove(item);
            await SaveAllAsync(items);
            return true;
        }
        return false;
    }

    private async Task SaveAllAsync<T>(List<T> items)
    {
        var fileName = GetFileNameForType<T>();
        await WriteJsonAsync(fileName, items);
    }

    private string GetFileNameForType<T>()
    {
        var typeName = typeof(T).Name.ToLower();
        return typeName switch
        {
            "user" => "users.json",
            "personalprofile" => "personalProfiles.json",
            "education" => "educations.json",
            "workexperience" => "workExperiences.json",
            "skill" => "skills.json",
            "portfolio" => "portfolios.json",
            "calendarevent" => "calendarEvents.json",
            "worktask" => "workTasks.json",
            "todoitem" => "todoItems.json",
            "blogpost" => "blogPosts.json",
            "guestbookentry" => "guestBookEntries.json",
            "contactmethod" => "contactMethods.json",
            _ => $"{typeName}s.json"
        };
    }

    private int GetId<T>(T item)
    {
        var idProperty = typeof(T).GetProperty("Id");
        return idProperty?.GetValue(item) as int? ?? 0;
    }

    private void SetId<T>(T item, int id)
    {
        var idProperty = typeof(T).GetProperty("Id");
        idProperty?.SetValue(item, id);
    }

    // Specific methods for each entity type
    public async Task<List<User>> GetUsersAsync()
    {
        return await ReadJsonAsync<User>("users.json");
    }

    public async Task SaveUsersAsync(List<User> users)
    {
        await WriteJsonAsync("users.json", users);
    }

    public async Task<List<PersonalProfile>> GetPersonalProfilesAsync()
    {
        return await ReadJsonAsync<PersonalProfile>("personalProfiles.json");
    }

    public async Task SavePersonalProfilesAsync(List<PersonalProfile> profiles)
    {
        await WriteJsonAsync("personalProfiles.json", profiles);
    }

    public async Task<List<Education>> GetEducationsAsync()
    {
        return await ReadJsonAsync<Education>("educations.json");
    }

    public async Task SaveEducationsAsync(List<Education> educations)
    {
        await WriteJsonAsync("educations.json", educations);
    }

    public async Task<List<WorkExperience>> GetWorkExperiencesAsync()
    {
        return await ReadJsonAsync<WorkExperience>("workExperiences.json");
    }

    public async Task SaveWorkExperiencesAsync(List<WorkExperience> workExperiences)
    {
        await WriteJsonAsync("workExperiences.json", workExperiences);
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        return await ReadJsonAsync<Skill>("skills.json");
    }

    public async Task SaveSkillsAsync(List<Skill> skills)
    {
        await WriteJsonAsync("skills.json", skills);
    }

    public async Task<List<Portfolio>> GetPortfoliosAsync()
    {
        return await ReadJsonAsync<Portfolio>("portfolios.json");
    }

    public async Task SavePortfoliosAsync(List<Portfolio> portfolios)
    {
        await WriteJsonAsync("portfolios.json", portfolios);
    }

    public async Task<List<CalendarEvent>> GetCalendarEventsAsync()
    {
        return await ReadJsonAsync<CalendarEvent>("calendarEvents.json");
    }

    public async Task SaveCalendarEventsAsync(List<CalendarEvent> events)
    {
        await WriteJsonAsync("calendarEvents.json", events);
    }

    public async Task<List<WorkTask>> GetWorkTasksAsync()
    {
        return await ReadJsonAsync<WorkTask>("workTasks.json");
    }

    public async Task SaveWorkTasksAsync(List<WorkTask> tasks)
    {
        await WriteJsonAsync("workTasks.json", tasks);
    }

    public async Task<List<TodoItem>> GetTodoItemsAsync()
    {
        return await ReadJsonAsync<TodoItem>("todoItems.json");
    }

    public async Task SaveTodoItemsAsync(List<TodoItem> todoItems)
    {
        await WriteJsonAsync("todoItems.json", todoItems);
    }

    public async Task<List<BlogPost>> GetBlogPostsAsync()
    {
        return await ReadJsonAsync<BlogPost>("blogPosts.json");
    }

    public async Task SaveBlogPostsAsync(List<BlogPost> blogPosts)
    {
        await WriteJsonAsync("blogPosts.json", blogPosts);
    }

    public async Task<List<GuestBookEntry>> GetGuestBookEntriesAsync()
    {
        return await ReadJsonAsync<GuestBookEntry>("guestBookEntries.json");
    }

    public async Task SaveGuestBookEntriesAsync(List<GuestBookEntry> entries)
    {
        await WriteJsonAsync("guestBookEntries.json", entries);
    }

    public async Task<List<ContactMethod>> GetContactMethodsAsync()
    {
        return await ReadJsonAsync<ContactMethod>("contactMethods.json");
    }

    public async Task SaveContactMethodsAsync(List<ContactMethod> contactMethods)
    {
        await WriteJsonAsync("contactMethods.json", contactMethods);
    }

    public async Task<List<FileUpload>> GetFileUploadsAsync()
    {
        return await ReadJsonAsync<FileUpload>("fileUploads.json");
    }

    public async Task SaveFileUploadsAsync(List<FileUpload> fileUploads)
    {
        await WriteJsonAsync("fileUploads.json", fileUploads);
    }

    // Helper methods for common operations
    public async Task<T?> GetByIdAsync<T>(string fileName, int id) where T : class
    {
        var items = await ReadJsonAsync<T>(fileName);
        var idProperty = typeof(T).GetProperty("Id");
        
        if (idProperty == null) return null;
        
        return items.FirstOrDefault(item => 
        {
            var itemId = idProperty.GetValue(item);
            return itemId != null && itemId.Equals(id);
        });
    }

    public async Task<List<T>> GetByUserIdAsync<T>(string fileName, int userId) where T : class
    {
        var items = await ReadJsonAsync<T>(fileName);
        var userIdProperty = typeof(T).GetProperty("UserId");
        
        if (userIdProperty == null) return new List<T>();
        
        return items.Where(item =>
        {
            var itemUserId = userIdProperty.GetValue(item);
            return itemUserId != null && itemUserId.Equals(userId);
        }).ToList();
    }
}