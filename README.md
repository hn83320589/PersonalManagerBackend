# Personal Manager Backend

個人展示與管理平台後端 API，使用 .NET 9.0 Web API 開發。

## 技術棧與套件

| 套件 | 版本 | 用途 |
|------|------|------|
| .NET SDK | 9.0 | 應用程式框架 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.13 | JWT 認證 |
| Swashbuckle.AspNetCore | 7.3.1 | Swagger/OpenAPI 文件 |
| BCrypt.Net-Next | 4.0.3 | 密碼雜湊 |
| Pomelo.EntityFrameworkCore.MySql | 9.0.0 | MariaDB/MySQL ORM（未來 DB 模式用） |
| Microsoft.EntityFrameworkCore.Design | 9.0.13 | EF Core 設計時工具 |
| xUnit | 2.9.2 | 單元測試 |
| coverlet.collector | 6.0.2 | 測試覆蓋率 |

## 系統架構

三層架構，所有層之間透過介面解耦，以依賴注入連接：

```
Controller (HTTP 處理)
    ↓ 呼叫
Service (業務邏輯)
    ↓ 呼叫
Repository (資料存取)
    ↓ 讀寫
JSON 檔案 (Data/JsonData/*.json)
```

### Repository 層

泛型 Repository 模式，以 JSON 檔案做持久化儲存：

```csharp
// 介面定義
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> FindAsync(Func<T, bool> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

`JsonRepository<T>` 為唯一實作，功能包含：
- 記憶體快取（首次讀取後快取整個 list）
- 自動 ID 遞增
- 自動設定 `CreatedAt` / `UpdatedAt` 時間戳
- JSON 檔案自動對映（型別名稱 → 檔案名稱，如 `User` → `users.json`）

在 DI 容器中註冊為 **Singleton**：

```csharp
builder.Services.AddSingleton<IRepository<User>, JsonRepository<User>>();
```

### Service 層

抽象基底類別 `CrudService<T, TCreate, TUpdate, TResponse>` 提供標準 CRUD：

```csharp
public abstract class CrudService<T, TCreate, TUpdate, TResponse>
    : ICrudService<T, TCreate, TUpdate, TResponse> where T : class
{
    protected abstract T MapToEntity(TCreate dto);
    protected abstract TResponse MapToResponse(T entity);
    protected abstract void ApplyUpdate(T entity, TUpdate dto);
}
```

每個實體服務繼承此基底，覆寫三個映射方法。部分服務另外擴充專屬方法（如 `IProfileService.GetByUserIdAsync`）。

共 13 個服務（1 個 Auth + 12 個實體服務），在 DI 容器中註冊為 **Scoped**：

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
// ... 其餘 11 個
```

### DTO 層

手動映射，定義在 `Mappings/MappingExtensions.cs`，使用擴充方法：

```csharp
// Model → Response DTO
public static UserResponse ToResponse(this User u) => new() { ... };

// Create DTO → Model
public static User ToEntity(this CreateUserDto d) => new() { ... };

// Update DTO → 套用到現有 Model
public static void ApplyUpdate(this User u, UpdateUserDto d) { ... }
```

### Controller 層

13 個 API Controller，統一使用 `ApiResponse<T>` 回應格式：

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; }
}
```

### Middleware

- `ErrorHandlingMiddleware`：全域例外捕獲，統一轉成 `ApiResponse` 格式回應

### 認證

- JWT Bearer Authentication（HS256 簽名）
- BCrypt 密碼雜湊（`AuthService`）
- 設定區段名稱為 `Jwt`（見 `appsettings.json`）

## 專案結構

```
PersonalManagerBackend/
├── PersonalManager.Api.csproj    # 專案檔
├── PersonalManager.sln           # 方案檔
├── Program.cs                    # 進入點、DI 註冊、Middleware 管線
├── appsettings.json              # 設定檔（JWT 等）
├── appsettings.Development.json  # 開發環境設定
├── Auth/                         # 認證
│   ├── AuthService.cs            #   IAuthService 實作（登入、註冊、Token 產生）
│   └── JwtSettings.cs            #   JWT 設定模型
├── Controllers/                  # 13 個 API Controller
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── ProfilesController.cs
│   ├── EducationsController.cs
│   ├── WorkExperiencesController.cs
│   ├── SkillsController.cs
│   ├── PortfoliosController.cs
│   ├── CalendarEventsController.cs
│   ├── TodoItemsController.cs
│   ├── WorkTasksController.cs
│   ├── BlogPostsController.cs
│   ├── GuestBookEntriesController.cs
│   └── ContactMethodsController.cs
├── DTOs/
│   ├── ApiResponse.cs            # 統一回應格式
│   ├── AuthDtos.cs               # 登入、註冊、Token DTO
│   └── EntityDtos.cs             # 12 組 Create/Update/Response DTO
├── Mappings/
│   └── MappingExtensions.cs      # DTO ↔ Model 手動映射
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
├── Models/                       # 12 個實體模型
│   ├── User.cs
│   ├── PersonalProfile.cs
│   ├── Education.cs, WorkExperience.cs, Skill.cs, Portfolio.cs
│   ├── CalendarEvent.cs, TodoItem.cs, WorkTask.cs
│   ├── BlogPost.cs, GuestBookEntry.cs, ContactMethod.cs
├── Repositories/
│   ├── IRepository.cs            # 泛型 Repository 介面
│   └── JsonRepository.cs         # JSON 檔案持久化實作
├── Services/
│   ├── CrudService.cs            # 抽象 CRUD 基底類別 + ICrudService 介面
│   └── EntityServices.cs         # 12 個實體服務實作
├── Data/JsonData/                # 12 個 JSON 資料檔
├── Properties/
│   └── launchSettings.json       # 啟動設定（port 5037）
├── sql/                          # 資料庫 schema SQL（未來用）
└── tests/
    └── PersonalManager.Api.Tests/  # xUnit 測試專案
```

## 如何執行

```bash
# 還原套件
dotnet restore

# 執行（開發模式）
dotnet run

# 或指定方案檔
dotnet run --project PersonalManager.Api.csproj
```

- API 位址：`http://localhost:5037`
- Swagger 文件：`http://localhost:5037/swagger`（僅開發模式）
- Demo 登入：帳號 `admin`，密碼 `demo123`

## 資料模式切換（JSON vs 資料庫）

### 目前狀態：JSON 模式（預設）

所有資料儲存在 `Data/JsonData/` 下的 JSON 檔案，不需要任何資料庫。`JsonRepository<T>` 負責讀寫這些檔案。

### 未來切換至資料庫模式

專案已包含 `Pomelo.EntityFrameworkCore.MySql` 套件與 `ApplicationDbContext`，未來可切換至 MariaDB/MySQL：

1. 在 `appsettings.json` 加入連線字串：
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=personal_manager;Uid=root;Pwd=your_password;"
     }
   }
   ```

2. 在 `Program.cs` 中註冊 `ApplicationDbContext` 並將 Repository 實作替換為 EF-based 版本

3. 建立並套用 Migration：
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### 設定檔結構

`appsettings.json` 目前的設定：

```json
{
  "Jwt": {
    "SecretKey": "your_secret_key_at_least_256_bits",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "ExpiryHours": 24
  }
}
```

注意設定區段名稱是 `Jwt`，對應 `JwtSettings` 類別。

## 如何修改 / 擴充

### 新增一個實體

1. **Model**：在 `Models/` 建立實體類別（需有 `int Id` 屬性）
2. **DTO**：在 `DTOs/EntityDtos.cs` 加入 `Create___Dto`、`Update___Dto`、`___Response`
3. **Mapping**：在 `Mappings/MappingExtensions.cs` 加入 `ToResponse()`、`ToEntity()`、`ApplyUpdate()` 擴充方法
4. **JSON 資料檔**：在 `Data/JsonData/` 建立對應的 JSON 檔案（空陣列 `[]`）
5. **Repository 註冊**：在 `Program.cs` 加入 `builder.Services.AddSingleton<IRepository<NewEntity>, JsonRepository<NewEntity>>()`
6. **Service**：在 `Services/EntityServices.cs` 加入 `INewEntityService` 介面與 `NewEntityService` 實作
7. **Service 註冊**：在 `Program.cs` 加入 `builder.Services.AddScoped<INewEntityService, NewEntityService>()`
8. **Controller**：在 `Controllers/` 建立 Controller，注入 Service

### 新增 API 端點

在對應的 Service 介面加入方法 → 實作 → Controller 加入 Action method。

## 建置與部署

```bash
# 建置
dotnet build PersonalManager.sln

# 發布（Release 模式）
dotnet publish -c Release -o ./publish

# 執行發布版本
dotnet ./publish/PersonalManager.Api.dll
```

### 生產環境設定

透過環境變數覆寫設定：

```bash
# JWT 密鑰（必填，至少 32 字元）
Jwt__SecretKey=your_production_secret_key

# 資料庫連線字串（切換至 DB 模式時）
ConnectionStrings__DefaultConnection=your_connection_string

# CORS 允許的前端 URL
# 目前寫死在 Program.cs，生產環境需修改
```

可部署至 Zeabur 或任何支援 .NET 9.0 的主機。

## 測試

```bash
# 執行所有測試
dotnet test PersonalManager.sln

# 測試專案位置
# tests/PersonalManager.Api.Tests/
```

## 相關連結

- [主專案倉庫](https://github.com/hn83320589/personal_manager)
- [前端專案倉庫](https://github.com/hn83320589/PersonalManagerFrontend)
