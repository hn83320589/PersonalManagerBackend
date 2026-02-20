using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Repositories;

public class JsonRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private List<T>? _cache;

    public JsonRepository(IWebHostEnvironment env)
    {
        var typeName = typeof(T).Name;
        var fileName = typeName switch
        {
            "PersonalProfile" => "personalProfiles",
            "Education" => "educations",
            "WorkExperience" => "workExperiences",
            "Skill" => "skills",
            "Portfolio" => "portfolios",
            "CalendarEvent" => "calendarEvents",
            "TodoItem" => "todoItems",
            "WorkTask" => "workTasks",
            "BlogPost" => "blogPosts",
            "GuestBookEntry" => "guestBookEntries",
            "ContactMethod" => "contactMethods",
            "User" => "users",
            _ => typeName.ToLowerInvariant() + "s"
        };

        _filePath = Path.Combine(env.ContentRootPath, "Data", "JsonData", $"{fileName}.json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private async Task<List<T>> LoadAsync()
    {
        if (_cache != null) return _cache;

        if (!File.Exists(_filePath))
        {
            _cache = [];
            return _cache;
        }

        var json = await File.ReadAllTextAsync(_filePath);
        _cache = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? [];
        return _cache;
    }

    private async Task SaveAsync(List<T> items)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
        _cache = items;
    }

    private static int GetId(T entity)
    {
        var prop = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        return prop != null ? (int)(prop.GetValue(entity) ?? 0) : 0;
    }

    private static void SetId(T entity, int id)
    {
        var prop = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        prop?.SetValue(entity, id);
    }

    public async Task<List<T>> GetAllAsync() => new(await LoadAsync());

    public async Task<T?> GetByIdAsync(int id)
    {
        var items = await LoadAsync();
        return items.FirstOrDefault(x => GetId(x) == id);
    }

    public async Task<List<T>> FindAsync(Func<T, bool> predicate)
    {
        var items = await LoadAsync();
        return items.Where(predicate).ToList();
    }

    public async Task<T> AddAsync(T entity)
    {
        var items = await LoadAsync();
        var maxId = items.Count > 0 ? items.Max(x => GetId(x)) : 0;
        SetId(entity, maxId + 1);

        // Set timestamps if available
        SetTimestamp(entity, "CreatedAt", DateTime.UtcNow);
        SetTimestamp(entity, "UpdatedAt", DateTime.UtcNow);

        items.Add(entity);
        await SaveAsync(items);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var items = await LoadAsync();
        var id = GetId(entity);
        var index = items.FindIndex(x => GetId(x) == id);
        if (index < 0) throw new KeyNotFoundException($"{typeof(T).Name} with Id {id} not found");

        SetTimestamp(entity, "UpdatedAt", DateTime.UtcNow);
        items[index] = entity;
        await SaveAsync(items);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var items = await LoadAsync();
        var removed = items.RemoveAll(x => GetId(x) == id);
        if (removed == 0) return false;
        await SaveAsync(items);
        return true;
    }

    private static void SetTimestamp(T entity, string propName, DateTime value)
    {
        var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(DateTime))
            prop.SetValue(entity, value);
    }
}
