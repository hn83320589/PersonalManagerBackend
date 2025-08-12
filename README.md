# Personal Manager Backend

這是Personal Manager系統的後端API專案，使用C# .NET Core Web API開發。

## 🚀 快速開始

### 前置需求

- .NET 9.0 SDK
- Visual Studio Code 或 Visual Studio
- (MariaDB/MySQL 資料庫 - 未來選項，目前使用 JSON 模擬資料)

### 安裝與執行

1. **Clone 專案**
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerBackend.git
   cd personal-manager-backend
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **資料存取設定**
   - 目前使用 JSON 模擬資料，無需資料庫設定
   - JSON 資料檔案位於 `Data/Json/` 目錄
   - 未來可更新 `appsettings.json` 連接真實資料庫

4. **執行專案**
   ```bash
   dotnet run
   ```

5. **存取 API**
   - API 基礎路徑: `http://localhost:5002/api`
   - Swagger 文檔: `http://localhost:5002/swagger`
   - 如果埠口衝突，可使用: `dotnet run --urls "http://localhost:5002"`

## 🛠️ 技術架構

- **框架**: ASP.NET Core 9.0 Web API
- **資料存取**: JsonDataService (目前) + Entity Framework Core 9.0.8 (未來)
- **資料庫**: JSON 模擬資料 + MariaDB 支援 (Pomelo.EntityFrameworkCore.MySql)
- **文檔**: Swagger/OpenAPI + 完整 API 文檔 + Postman Collection
- **測試**: xUnit 基礎測試 + 手動整合測試

## 📁 專案結構

```
PersonalManagerBackend/
├── Controllers/          # API 控制器
├── Models/              # 資料模型
├── Services/            # 業務邏輯服務
├── Data/               # 資料存取層
├── DTOs/               # 資料傳輸物件
├── Middleware/         # 中介軟體
├── Configuration/      # 設定相關
├── DB/                # 資料庫設計檔案
├── Program.cs         # 程式進入點
└── CLAUDE.md          # 開發文檔
```

## 🔧 開發指令

```bash
# 建置專案
dotnet build

# 執行專案
dotnet run

# 執行測試
dotnet test

# 建立 Migration
dotnet ef migrations add MigrationName

# 更新資料庫
dotnet ef database update

# 新增套件
dotnet add package PackageName
```

## 🌟 主要功能

- [x] 基本API架構設定
- [x] Entity Framework Core 設定 + JsonDataService
- [x] Swagger API 文檔 + 完整技術文件
- [x] CORS 跨域設定
- [x] 使用者管理 API (UsersController)
- [x] 個人資料管理 API (PersonalProfilesController)
- [x] 學經歷管理 API (EducationsController + WorkExperiencesController)
- [x] 技能管理 API (SkillsController)
- [x] 作品集 API (PortfoliosController)
- [x] 行事曆管理 API (CalendarEventsController)
- [x] 任務管理 API (TodoItemsController + WorkTasksController)
- [x] 部落格管理 API (BlogPostsController)
- [x] 留言管理 API (GuestBookEntriesController)
- [x] 聯絡方式 API (ContactMethodsController)
- [x] 檔案上傳 API (FilesController)
- [ ] JWT 身份驗證 (計劃中)

## 📋 API 端點概覽

### 核心功能 API (已完成)
- `GET /api/users` - 使用者管理 (CRUD)
- `GET /api/personalprofiles` - 個人資料管理
- `GET /api/educations` - 學歷管理
- `GET /api/workexperiences` - 工作經歷管理
- `GET /api/skills` - 技能管理
- `GET /api/portfolios` - 作品集管理
- `GET /api/calendarevents` - 行事曆管理
- `GET /api/todoitems` - 待辦事項管理
- `GET /api/worktasks` - 工作追蹤
- `GET /api/blogposts` - 部落格管理
- `GET /api/guestbookentries` - 留言板管理
- `GET /api/contactmethods` - 聯絡方式管理
- `POST /api/files/upload` - 檔案上傳

### 認證 API (計劃中)
- `POST /api/auth/login` - 使用者登入
- `POST /api/auth/logout` - 使用者登出

**詳細API文檔**: 
- [完整API技術文檔](../../docs/api-documentation.md)
- [API快速參考](../../docs/api-quick-reference.md)
- [Postman Collection](../../docs/PersonalManager-API.postman_collection.json)
- [Swagger文檔](http://localhost:5002/swagger)

## 🔒 環境設定

### 必要環境變數

```env
ASPNETCORE_ENVIRONMENT=Development
CONNECTION_STRINGS__DEFAULT=Server=localhost;Database=personal_manager;Uid=root;Pwd=your_password;
JWT_SECRET=your_jwt_secret_key
```

### appsettings.json 範例

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=personal_manager;Uid=root;Pwd=your_password;"
  }
}
```

## 🧪 測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試
dotnet test --filter "TestClassName"

# 生成測試覆蓋率報告
dotnet test --collect:"XPlat Code Coverage"
```

## 📦 部署

### Docker 部署

```dockerfile
# Dockerfile 內容將在部署階段建立
```

### Zeabur 部署

1. 連接 GitHub 倉庫到 Zeabur
2. 設定環境變數
3. 自動部署

## 🤝 開發規範

- 遵循 RESTful API 設計原則
- 所有 API 都需要適當的錯誤處理
- 使用 Data Annotations 進行資料驗證
- 撰寫單元測試覆蓋重要功能
- 敏感資訊使用環境變數或 User Secrets

## 📞 相關連結

- [主專案倉庫](https://github.com/hn83320589/personal_manager)
- [前端專案倉庫](https://github.com/hn83320589/PersonalManagerFrontend)
- [專案文檔](https://github.com/hn83320589/personal_manager/blob/main/docs/)

## 📄 授權

MIT License - 詳見 [LICENSE](LICENSE) 檔案。