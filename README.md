# Personal Manager Backend

個人展示與管理平台後端 API，使用 .NET 9.0 Web API 開發。

## 技術棧與套件

| 套件 | 版本 | 用途 |
|------|------|------|
| .NET SDK | 9.0 | 應用程式框架 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.13 | JWT 認證 |
| Swashbuckle.AspNetCore | 7.3.1 | Swagger/OpenAPI 文件 |
| BCrypt.Net-Next | 4.0.3 | 密碼雜湊 |
| Pomelo.EntityFrameworkCore.MySql | 9.0.0 | MariaDB/MySQL ORM |
| Microsoft.EntityFrameworkCore.Design | 9.0.13 | EF Core 設計時工具 |

## 系統架構

三層架構，所有層之間透過介面解耦，以依賴注入連接：

```
Controller (HTTP 處理)
    ↓ 呼叫
Service (業務邏輯)
    ↓ 呼叫
IRepository<T> (資料存取介面)
    ├── EfRepository<T>  → MariaDB（有 DB 連線時）
    └── JsonRepository<T> → Data/JsonData/*.json（無 DB 時 fallback）
```

啟動時自動偵測 DB 連線（`ServerVersion.AutoDetect()`），根據結果選擇對應實作。

### Repository 層

泛型 Repository 介面：

```csharp
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

兩個實作：

| 實作 | 使用時機 | 說明 |
|------|----------|------|
| `EfRepository<T>` | DB 可連線 | EF Core + MariaDB，`EnsureCreated()` 自動建表 |
| `JsonRepository<T>` | DB 無法連線 | 讀寫 `Data/JsonData/*.json`，自動 ID 遞增、時間戳 |

在 DI 容器中依 DB 狀態擇一註冊為 **Scoped**：

```csharp
// DB 可連線
builder.Services.AddScoped<IRepository<User>, EfRepository<User>>();
// DB 無法連線
builder.Services.AddScoped<IRepository<User>, JsonRepository<User>>();
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
│   ├── EfRepository.cs           # EF Core 實作（DB 模式）
│   └── JsonRepository.cs         # JSON 檔案實作（fallback 模式）
├── Services/
│   ├── CrudService.cs            # 抽象 CRUD 基底類別 + ICrudService 介面
│   └── EntityServices.cs         # 12 個實體服務實作
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext（12 個 DbSet）
│   ├── DatabaseSeeder.cs         # 索引建立 + 初始種子資料
│   └── JsonData/                 # JSON fallback 資料檔（*.json）
├── Properties/
│   └── launchSettings.json       # 啟動設定（port 5037）
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

## 資料模式（自動切換）

啟動時自動偵測 `appsettings.json` 中的 `ConnectionStrings.DefaultConnection`：

| 情況 | 使用模式 | 啟動 log |
|------|----------|----------|
| 連線成功 | EF Core + MariaDB | `啟動模式: 資料庫 (MariaDB)` |
| 連線失敗 / 連線字串為空 | JSON fallback | `啟動模式: JSON Fallback` |

DB 模式會自動執行 `EnsureCreated()` 建立資料表與索引，不需要手動跑 migrations。

### 設定檔結構（`appsettings.json`）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=personal_manager;..."
  },
  "Jwt": {
    "SecretKey": "your_secret_key_at_least_256_bits",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "ExpiryHours": 24
  }
}
```

注意設定區段名稱是 `Jwt`（非 `JwtSettings`）。若要強制使用 JSON 模式，將 `DefaultConnection` 設為空字串即可。

## 如何修改 / 擴充

### 新增一個實體

1. **Model**：在 `Models/` 建立實體類別（需有 `int Id` 屬性）
2. **DTO**：在 `DTOs/EntityDtos.cs` 加入 `Create___Dto`、`Update___Dto`、`___Response`
3. **Mapping**：在 `Mappings/MappingExtensions.cs` 加入 `ToResponse()`、`ToEntity()`、`ApplyUpdate()` 擴充方法
4. **JSON 資料檔**：在 `Data/JsonData/` 建立對應的 JSON 檔案（空陣列 `[]`）
5. **Repository 註冊**：在 `Program.cs` 的兩個 if 區塊各加一行（EF 和 JSON 各一）
6. **Service**：在 `Services/EntityServices.cs` 加入 `INewEntityService` 介面與 `NewEntityService` 實作
7. **Service 註冊**：在 `Program.cs` 加入 `builder.Services.AddScoped<INewEntityService, NewEntityService>()`
8. **Controller**：在 `Controllers/` 建立 Controller，注入 Service

### 新增 API 端點

在對應的 Service 介面加入方法 → 實作 → Controller 加入 Action method。

## 建置與部署

```bash
# 建置
dotnet build PersonalManager.Api.csproj

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
dotnet test PersonalManager.Api.csproj

# 測試專案位置
# tests/PersonalManager.Api.Tests/
```

## 相關連結

- [主專案倉庫](https://github.com/hn83320589/personal_manager)
- [前端專案倉庫](https://github.com/hn83320589/PersonalManagerFrontend)
