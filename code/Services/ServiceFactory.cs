using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Services.Implementation;

namespace PersonalManagerAPI.Services;

/// <summary>
/// 服務工廠 - 根據配置選擇適當的服務實作
/// </summary>
public class ServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ServiceFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// 判斷是否使用 Entity Framework
    /// </summary>
    public bool UseEntityFramework => 
        _configuration.GetValue<bool>("UseEntityFramework", false) ||
        !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection"));

    /// <summary>
    /// 取得使用者服務實作
    /// </summary>
    public IUserService GetUserService()
    {
        if (UseEntityFramework)
        {
            return _serviceProvider.GetRequiredService<UserServiceEF>();
        }
        else
        {
            return _serviceProvider.GetRequiredService<UserService>();
        }
    }

    /// <summary>
    /// 取得個人資料服務實作
    /// </summary>
    public IPersonalProfileService GetPersonalProfileService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<PersonalProfileService>();
    }

    /// <summary>
    /// 取得學歷服務實作
    /// </summary>
    public IEducationService GetEducationService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<EducationService>();
    }

    /// <summary>
    /// 取得技能服務實作
    /// </summary>
    public ISkillService GetSkillService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<SkillService>();
    }

    /// <summary>
    /// 取得工作經歷服務實作
    /// </summary>
    public IWorkExperienceService GetWorkExperienceService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<WorkExperienceService>();
    }

    /// <summary>
    /// 取得作品集服務實作
    /// </summary>
    public IPortfolioService GetPortfolioService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<PortfolioService>();
    }

    /// <summary>
    /// 取得行事曆服務實作
    /// </summary>
    public ICalendarEventService GetCalendarEventService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<CalendarEventService>();
    }

    /// <summary>
    /// 取得待辦事項服務實作
    /// </summary>
    public ITodoItemService GetTodoItemService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<TodoItemService>();
    }

    /// <summary>
    /// 取得工作任務服務實作
    /// </summary>
    public IWorkTaskService GetWorkTaskService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<WorkTaskService>();
    }

    /// <summary>
    /// 取得部落格服務實作
    /// </summary>
    public IBlogPostService GetBlogPostService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<BlogPostService>();
    }

    /// <summary>
    /// 取得留言板服務實作
    /// </summary>
    public IGuestBookService GetGuestBookService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<GuestBookService>();
    }

    /// <summary>
    /// 取得聯絡方式服務實作
    /// </summary>
    public IContactMethodService GetContactMethodService()
    {
        // TODO: 實作 Entity Framework 版本後，根據配置返回適當的實作
        return _serviceProvider.GetRequiredService<ContactMethodService>();
    }
}