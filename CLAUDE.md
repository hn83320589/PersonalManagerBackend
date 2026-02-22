# CLAUDE.md - PersonalManager Backend

This file provides guidance to Claude Code when working with the backend codebase.

---

## 快速啟動

```bash
# 開發模式啟動 (自動偵測 DB 連線，無 DB 則 fallback JSON)
dotnet run

# API 服務預設跑在:
# http://localhost:5037
# Swagger UI: http://localhost:5037/swagger
```

### 注意事項

- **.NET SDK 版本**：需要 .NET 9.0。若出現 `Roll Forward` 錯誤，執行：
  ```bash
  DOTNET_ROLL_FORWARD=LatestMajor dotnet run
  ```
- **DB 連線**：若 `appsettings.json` 的 `ConnectionStrings.DefaultConnection` 可連線，自動使用 MariaDB（EF Core）；無法連線時自動 fallback 至 `Data/JsonData/*.json`。
- **不需要執行 migrations**：`EnsureCreated()` 在每次啟動時自動建立資料表（只在 DB 模式有效）。

---

## 架構說明

### 技術棧

| 項目 | 版本/工具 |
|------|-----------|
| 框架 | .NET 9.0 Web API |
| ORM | Entity Framework Core 9 + Pomelo.EntityFrameworkCore.MySql 9.0 |
| 資料庫 | MariaDB（生產）/ 本地 JSON 檔案（開發 fallback） |
| 認證 | JWT Bearer Token |
| API 文件 | Swagger / OpenAPI |
| 密碼雜湊 | BCrypt.Net-Next |

### 請求流程

```
HTTP 請求
  → ErrorHandlingMiddleware（統一錯誤處理）
  → JWT 認證/授權
  → Controller（接收請求，回傳 ApiResponse<T>）
  → Service（業務邏輯，使用 DTO）
  → IRepository<T>（資料存取介面）
      ├── EfRepository<T>（有 DB 時：EF Core → MariaDB）
      └── JsonRepository<T>（無 DB 時：讀寫 Data/JsonData/*.json）
```

### DB 自動偵測機制（`Program.cs`）

啟動時嘗試 `ServerVersion.AutoDetect(connectionString)`：
- **成功** → 使用 `EfRepository<T>`，執行 `EnsureCreated()` + `DatabaseSeeder`
- **失敗** → 使用 `JsonRepository<T>`，讀寫本地 JSON 檔案

啟動 log 會顯示：
- `啟動模式: 資料庫 (MariaDB)` — DB 模式
- `啟動模式: JSON Fallback` — JSON 模式

### JSON 序列化規則

- **camelCase**：所有 JSON 欄位名稱使用 camelCase（`PropertyNamingPolicy.CamelCase`）
- **Enum 為字串**：Enum 序列化為字串（如 `"Published"`、`"Expert"`）
- 前端 TypeScript 介面與此完全對應

---

## 專案結構

```
PersonalManagerBackend/
├── Program.cs                    # 進入點：DI、DB 偵測、Middleware
├── appsettings.json              # 設定（包含 DB 連線字串與 JWT）
├── appsettings.Development.json  # 開發環境補充設定
├── PersonalManager.Api.csproj    # 專案檔
│
├── Auth/
│   ├── AuthService.cs            # JWT token 產生、使用者驗證
│   └── JwtSettings.cs            # JWT 設定 model
│
├── Controllers/                  # 12 個 API 控制器
│   ├── AuthController.cs         # POST /api/auth/login, /register
│   ├── UsersController.cs        # /api/users
│   ├── ProfilesController.cs     # /api/profiles
│   ├── EducationsController.cs   # /api/educations
│   ├── WorkExperiencesController.cs # /api/workexperiences
│   ├── SkillsController.cs       # /api/skills
│   ├── PortfoliosController.cs   # /api/portfolios
│   ├── CalendarEventsController.cs  # /api/calendarevents
│   ├── TodoItemsController.cs    # /api/todoitems
│   ├── WorkTasksController.cs    # /api/worktasks
│   ├── BlogPostsController.cs    # /api/blogposts
│   ├── GuestBookEntriesController.cs # /api/guestbookentries
│   └── ContactMethodsController.cs  # /api/contactmethods
│
├── DTOs/
│   ├── ApiResponse.cs            # 統一回應格式 ApiResponse<T>
│   ├── AuthDtos.cs               # LoginRequest, RegisterRequest, AuthResponse
│   └── EntityDtos.cs             # 所有實體的 Create/Update/Response DTOs
│
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext（12 個 DbSet）
│   ├── DatabaseSeeder.cs         # 初始資料種子 + 索引建立
│   └── JsonData/                 # JSON fallback 資料檔案（*.json）
│
├── Mappings/
│   └── MappingExtensions.cs      # Model ↔ DTO 手動映射擴展方法
│
├── Middleware/
│   └── ErrorHandlingMiddleware.cs # 全域例外處理，統一回傳 ApiResponse
│
├── Models/                       # 12 個 EF Core 實體
│   ├── User.cs
│   ├── PersonalProfile.cs
│   ├── Education.cs
│   ├── WorkExperience.cs
│   ├── Skill.cs
│   ├── Portfolio.cs
│   ├── CalendarEvent.cs
│   ├── TodoItem.cs
│   ├── WorkTask.cs
│   ├── BlogPost.cs
│   ├── GuestBookEntry.cs
│   └── ContactMethod.cs
│
├── Repositories/
│   ├── IRepository.cs            # 通用 CRUD 介面
│   ├── EfRepository.cs           # EF Core 實作（DB 模式）
│   └── JsonRepository.cs         # JSON 檔案實作（fallback 模式）
│
└── Services/
    ├── CrudService.cs            # 通用 CRUD 業務邏輯基礎類別
    └── EntityServices.cs         # 12 個實體的具體服務類別
```

---

## API 路由總覽

| Controller | 路由前綴 | 說明 |
|------------|----------|------|
| AuthController | `/api/auth` | 登入、註冊 |
| UsersController | `/api/users` | 使用者管理 |
| ProfilesController | `/api/profiles` | 個人資料 |
| EducationsController | `/api/educations` | 學歷 |
| WorkExperiencesController | `/api/workexperiences` | 工作經歷 |
| SkillsController | `/api/skills` | 技能 |
| PortfoliosController | `/api/portfolios` | 作品集 |
| CalendarEventsController | `/api/calendarevents` | 行事曆 |
| TodoItemsController | `/api/todoitems` | 待辦事項 |
| WorkTasksController | `/api/worktasks` | 工作追蹤 |
| BlogPostsController | `/api/blogposts` | 部落格文章 |
| GuestBookEntriesController | `/api/guestbookentries` | 留言板 |
| ContactMethodsController | `/api/contactmethods` | 聯絡方式 |

所有資料回應格式：
```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": null
}
```

---

## 設定檔說明

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=personal_manager;..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "ExpiryHours": 24
  }
}
```

- 若要在本地使用 JSON fallback，可將 `DefaultConnection` 設為空字串。
- JWT 設定區段名稱為 `Jwt`（不是 `JwtSettings`）。

### `appsettings.Development.json`

開發環境的補充設定，不覆蓋 `appsettings.json` 的連線字串。

---

## 如何新增一個實體

1. **新增 Model**（`Models/NewEntity.cs`）：
   ```csharp
   public class NewEntity
   {
       public int Id { get; set; }
       public int UserId { get; set; }
       public string Name { get; set; } = string.Empty;
       public DateTime CreatedAt { get; set; }
       public DateTime UpdatedAt { get; set; }
   }
   ```

2. **新增 DTO**（`DTOs/EntityDtos.cs`）：
   ```csharp
   public class CreateNewEntityDto { ... }
   public class UpdateNewEntityDto { ... }
   public class NewEntityResponseDto { ... }
   ```

3. **新增映射**（`Mappings/MappingExtensions.cs`）：
   ```csharp
   public static NewEntityResponseDto ToResponseDto(this NewEntity entity) { ... }
   public static NewEntity ToModel(this CreateNewEntityDto dto) { ... }
   ```

4. **新增 DbSet**（`Data/ApplicationDbContext.cs`）：
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

5. **新增 JSON 資料檔**（`Data/JsonData/NewEntities.json`）：
   ```json
   []
   ```

6. **新增 Service**（`Services/EntityServices.cs`）：
   ```csharp
   public interface INewEntityService : ICrudService<NewEntity, CreateNewEntityDto, UpdateNewEntityDto, NewEntityResponseDto> { }
   public class NewEntityService : CrudService<NewEntity, CreateNewEntityDto, UpdateNewEntityDto, NewEntityResponseDto>, INewEntityService
   {
       public NewEntityService(IRepository<NewEntity> repo) : base(repo) { }
       protected override NewEntityResponseDto ToResponseDto(NewEntity entity) => entity.ToResponseDto();
       protected override NewEntity ToModel(CreateNewEntityDto dto) => dto.ToModel();
       protected override void UpdateModel(NewEntity entity, UpdateNewEntityDto dto) => dto.ApplyTo(entity);
   }
   ```

7. **在 `Program.cs` 中 DI 兩次**（EF 和 JSON 兩個 if 區塊各加一行）：
   ```csharp
   builder.Services.AddScoped<IRepository<NewEntity>, EfRepository<NewEntity>>();
   // 以及
   builder.Services.AddScoped<IRepository<NewEntity>, JsonRepository<NewEntity>>();
   // Service 只加一次（共用）：
   builder.Services.AddScoped<INewEntityService, NewEntityService>();
   ```

8. **新增 Controller**（`Controllers/NewEntitiesController.cs`）：
   參考 `SkillsController.cs` 的結構，注入 `INewEntityService`。

---

## 開發注意事項

- **永遠不要使用 `--no-verify`** 繞過 commit hooks
- **不要 disable 測試**，修復它
- **commit 前先確認 `dotnet build` 通過**
- **Model 屬性異動後**，JSON fallback 的 `.json` 資料可能需要更新欄位
- **EF 模式**下，`EnsureCreated()` 自動處理 schema，不需要手動執行 migrations

---

## 最新異動記錄

### 2026/02/22
- **JSON 序列化修復**：`PropertyNamingPolicy` 從 `null`（PascalCase）改為 `CamelCase`，確保前後端欄位名稱一致
- **DB 自動偵測 fallback**：`Program.cs` 新增啟動時 DB 連線探測，無法連線時自動使用本地 JSON 資料
- **移除 `sql/` 資料夾**：SQL 建表腳本由 EF Core `EnsureCreated()` + `DatabaseSeeder.cs` 取代
- **前後端欄位對齊驗證**：所有 12 個實體的 DTO 欄位與前端 TypeScript 介面確認完全一致
