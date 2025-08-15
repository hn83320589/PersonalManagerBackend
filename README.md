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
   - API 基礎路徑: `http://localhost:5253/api`
   - Swagger 文檔: `http://localhost:5253/swagger`
   - 如果埠口衝突，可使用: `dotnet run --urls "http://localhost:5253"`

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

### ✅ 第一期已完成 (100%)
- [x] **基本API架構**: ASP.NET Core 9.0 + Entity Framework Core
- [x] **JsonDataService**: 完整的JSON模擬資料管理
- [x] **API文檔系統**: Swagger + 技術文檔 + Postman Collection
- [x] **CORS跨域支援**: 前後端整合配置
- [x] **統一回應格式**: ApiResponse標準化
- [x] **完整API Controllers** (13個):
  - [x] 使用者管理 API (UsersController) - 5個端點
  - [x] 個人資料管理 API (PersonalProfilesController) - 6個端點
  - [x] 學歷管理 API (EducationsController) - 6個端點
  - [x] 工作經歷管理 API (WorkExperiencesController) - 7個端點
  - [x] 技能管理 API (SkillsController) - 8個端點
  - [x] 作品集管理 API (PortfoliosController) - 7個端點
  - [x] 行事曆管理 API (CalendarEventsController) - 6個端點
  - [x] 待辦事項管理 API (TodoItemsController) - 6個端點
  - [x] 工作追蹤 API (WorkTasksController) - 5個端點
  - [x] 部落格管理 API (BlogPostsController) - 5個端點
  - [x] 留言管理 API (GuestBookEntriesController) - 4個端點
  - [x] 聯絡方式管理 API (ContactMethodsController) - 3個端點
  - [x] 檔案上傳 API (FilesController) - 3個端點
- [x] **整合測試**: 手動API測試完成，所有端點驗證通過
- [x] **前後端整合**: CORS配置完成，API整合100%成功

### 🚀 第二期規劃 (服務層重構與優化)
- [ ] **服務層架構**: 12個Service interfaces + 實作
- [ ] **DTOs體系**: 36個DTOs (Create/Update/Response)
- [ ] **JWT身份驗證**: Token管理 + 權限控制
- [ ] **Entity Framework**: 資料庫Migration + 查詢優化
- [ ] **Redis快取**: 分散式快取 + Session管理
- [ ] **第三方整合**: Google Calendar + OAuth + 檔案儲存

## 📋 API 端點統計

### 總API端點: 65+ 個 (已完成)
```
使用者與認證:
├── Users API (5個端點): CRUD操作、使用者檢查
├── PersonalProfiles API (6個端點): 個人資料、公開/私人設定

學經歷管理:
├── Educations API (6個端點): 學歷CRUD、排序、公開篩選
├── WorkExperiences API (7個端點): 工作經歷、目前職位、公司查詢

技能與作品:
├── Skills API (8個端點): 技能管理、分類、等級篩選、統計
├── Portfolios API (7個端點): 作品集、技術篩選、特色專案

時間管理:
├── CalendarEvents API (6個端點): 行事曆、公開/私人事件、日期範圍
├── TodoItems API (6個端點): 待辦事項、狀態管理、優先級、到期提醒
├── WorkTasks API (5個端點): 工作追蹤、專案分組、時間記錄

內容管理:
├── BlogPosts API (5個端點): 部落格、發布狀態、搜尋、分頁
├── GuestBookEntries API (4個端點): 留言管理、回覆、分頁、搜尋
├── ContactMethods API (3個端點): 聯絡方式、類型分類、社群整合

檔案管理:
└── Files API (3個端點): 檔案上傳、驗證、儲存管理
```

### 🔗 API 文檔資源
- [完整API技術文檔](../../docs/api-documentation.md) - 35KB詳細說明
- [API快速參考](../../docs/api-quick-reference.md) - 8KB開發參考
- [Postman Collection](../../docs/PersonalManager-API.postman_collection.json) - 完整測試集合
- [Swagger在線文檔](http://localhost:5253/swagger) - 互動式API測試

### 🚀 第二期API規劃
- **認證API**: JWT登入/登出、Token刷新、權限驗證
- **GraphQL支援**: 查詢優化、批量操作
- **WebSocket API**: 即時通知、聊天功能
- **第三方整合**: Google Calendar、OAuth、檔案儲存

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