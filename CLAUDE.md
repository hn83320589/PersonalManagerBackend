# CLAUDE.md - PersonalManager Backend

This file provides guidance to Claude Code (claude.ai/code) when working with the backend code in this repository.

## 專案說明

這是Personal Manager系統的後端專案，使用C# .NET Core Web API開發，搭配Entity Framework Core與MariaDB資料庫。

## 技術架構

- **框架**: .NET 9.0 Web API
- **資料庫**: MariaDB with Entity Framework Core
- **ORM**: Entity Framework Core 9.0.8
- **API文件**: Swagger/OpenAPI
- **套件管理**: NuGet

## 專案結構

```
PersonalManagerBackend/
├── Controllers/           # API控制器
│   └── BaseController.cs # 控制器基礎類別
├── Models/               # 資料模型
├── Services/             # 業務邏輯服務
├── Data/                # 資料存取層
│   └── ApplicationDbContext.cs
├── DTOs/                # 資料傳輸物件
├── Middleware/          # 中介軟體
├── Configuration/       # 設定相關
├── DB/                 # 資料庫設計檔案
├── Program.cs          # 程式進入點
└── appsettings.json    # 應用程式設定
```

## 開發規範

### 命名規範
- 控制器: `{Entity}Controller` (例: UserController)
- 服務: `I{Entity}Service` / `{Entity}Service`
- 模型: PascalCase (例: User, BlogPost)
- DTO: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`

### API路由規範
- 基本路由: `api/[controller]`
- RESTful設計原則
- 使用適當的HTTP動詞 (GET, POST, PUT, DELETE)

## 資料庫設計

### 主要實體
1. **User** - 使用者資料
2. **Profile** - 個人檔案
3. **Experience** - 學經歷
4. **Skill** - 專長技能
5. **Project** - 作品專案
6. **Calendar** - 行事曆
7. **Task** - 待辦事項
8. **WorkTracking** - 工作追蹤
9. **BlogPost** - 文章/網誌
10. **Comment** - 留言
11. **Contact** - 聯絡資訊

## 待辦事項追蹤

### 資料庫與模型開發
- [x] 設計資料庫架構 (ERD) - 已完成SQL設計檔案
- [ ] 建立資料庫 Migration 檔案 - 目前使用JSON模擬資料
- [x] 建立 User 模型與相關設定
- [x] 建立 PersonalProfile 模型 (個人介紹)
- [x] 建立 Education 模型 (學歷)
- [x] 建立 WorkExperience 模型 (工作經歷)
- [x] 建立 Skill 模型 (專長)
- [x] 建立 Portfolio 模型 (作品)
- [x] 建立 CalendarEvent 模型 (行事曆)
- [x] 建立 TodoItem 模型 (待辦事項)
- [x] 建立 WorkTask 模型 (工作追蹤)
- [x] 建立 BlogPost 模型 (文章/網誌)
- [x] 建立 GuestBookEntry 模型 (留言)
- [x] 建立 ContactMethod 模型 (聯絡資訊)
- [x] 設定模型間的關聯關係
- [x] 建立JSON模擬資料檔案 (替代種子資料)

### API開發
- [ ] 建立 AuthController (身份驗證)
- [x] 建立 UsersController (使用者管理) - 完整CRUD操作
- [x] 建立 PersonalProfilesController (個人資料) - 支援公開/私人、使用者查詢
- [x] 建立 EducationsController (學歷) - 支援排序、公開篩選
- [x] 建立 WorkExperiencesController (工作經歷) - 支援目前職位查詢
- [x] 建立 SkillsController (專長) - 支援分類、等級篩選
- [x] 建立 PortfoliosController (作品) - 支援技術篩選、功能型專案查詢
- [x] 建立 CalendarEventsController (行事曆) - 支援公開/私人事件、日期範圍查詢
- [x] 建立 TodoItemsController (待辦事項) - 支援狀態、優先順序篩選、到期提醒
- [x] 建立 WorkTasksController (工作追蹤) - 支援專案分組、時間追蹤、進度管理
- [x] 建立 BlogPostsController (文章/網誌) - 支援發佈狀態、搜尋、分頁功能
- [x] 建立 GuestBookEntriesController (留言) - 支援回覆功能、分頁、關鍵字搜尋
- [x] 建立 ContactMethodsController (聯絡資訊) - 支援類型分類、社群媒體整合
- [x] 建立 ApiResponse 統一回應格式
- [x] 設定 JsonDataService 依賴注入
- [x] API 基礎測試驗證

### 服務層開發
- [ ] 建立 IUserService 與實作
- [ ] 建立 IProfileService 與實作
- [ ] 建立 IExperienceService 與實作
- [ ] 建立 ISkillService 與實作
- [ ] 建立 IProjectService 與實作
- [ ] 建立 ICalendarService 與實作
- [ ] 建立 ITaskService 與實作
- [ ] 建立 IWorkTrackingService 與實作
- [ ] 建立 IBlogService 與實作
- [ ] 建立 ICommentService 與實作
- [ ] 建立 IContactService 與實作

### DTOs開發
- [ ] User相關 DTOs (UserDto, CreateUserDto, UpdateUserDto)
- [ ] Profile相關 DTOs
- [ ] Experience相關 DTOs
- [ ] Skill相關 DTOs
- [ ] Project相關 DTOs
- [ ] Calendar相關 DTOs
- [ ] Task相關 DTOs
- [ ] WorkTracking相關 DTOs
- [ ] BlogPost相關 DTOs
- [ ] Comment相關 DTOs
- [ ] Contact相關 DTOs

### 驗證與安全性
- [ ] 實作 JWT Token 驗證
- [ ] 建立 Authorization 中介軟體
- [ ] 實作資料驗證 (Data Annotations)
- [ ] 實作錯誤處理中介軟體
- [ ] 設定 CORS 政策

### 測試開發
- [x] 設定單元測試專案 - 建立基礎測試框架
- [x] 撰寫 Controller 單元測試 - 建立 BasicTests.cs
- [x] 撰寫 Service 單元測試 - 包含 JsonDataService 測試
- [x] 撰寫整合測試 - 完成手動 API 整合測試
- [x] 設定測試資料庫 - 使用 JSON 檔案模擬資料

### 部署準備
- [ ] 設定 Docker 容器化
- [ ] 準備 Production 環境設定
- [ ] 建立 CI/CD 管線設定
- [x] 撰寫 API 文件 - 完成詳細API文檔、快速參考手冊、Postman Collection

## 常用指令

### Entity Framework
```bash
# 建立 Migration
dotnet ef migrations add InitialCreate

# 更新資料庫
dotnet ef database update

# 移除最後一個 Migration
dotnet ef migrations remove
```

### 專案管理
```bash
# 建置專案
dotnet build

# 執行專案
dotnet run

# 執行測試
dotnet test

# 新增套件
dotnet add package PackageName
```

## 開發紀錄

### 2025/08/12 - 完成後端 API 開發
- 完成所有剩餘的 API Controllers：
  - **PortfoliosController** - 作品管理，支援技術篩選、特色作品查詢
  - **CalendarEventsController** - 行事曆管理，支援公開/私人事件、日期範圍查詢
  - **TodoItemsController** - 待辦事項管理，支援狀態、優先順序篩選、到期提醒
  - **WorkTasksController** - 工作追蹤，支援專案分組、時間追蹤、進度管理
  - **BlogPostsController** - 文章/網誌管理，支援發佈狀態、搜尋、分頁功能
  - **GuestBookEntriesController** - 留言管理，支援回覆功能、分頁、關鍵字搜尋
  - **ContactMethodsController** - 聯絡資訊管理，支援類型分類、社群媒體整合
- 修正了所有 Model 屬性不匹配問題：
  - TodoItem 新增 Status 和 CompletedAt 屬性
  - WorkTask 新增 ProjectId 和 CompletedAt 屬性
  - CalendarEvent 新增 StartTime、EndTime、Color 屬性
  - BlogPost 新增 PublishedAt 和 Slug 屬性
  - GuestBookEntry 新增 ParentId 屬性
  - Portfolio 新增 Technologies 和 RepositoryUrl 屬性
  - ContactMethod 新增 Icon 屬性
- 修正 TaskStatus 枚舉，新增 Pending 狀態
- 修正所有 Controller 的類型不匹配問題
- 建置成功，所有 API 端點正常運作

### 2025/08/12 - API文件完成
- 完成完整的API技術文檔：
  - **api-documentation.md** - 詳細API文檔，包含所有端點說明、請求/回應範例、資料模型定義
  - **api-quick-reference.md** - 快速參考手冊，提供簡潔的API端點總覽和測試指令
  - **PersonalManager-API.postman_collection.json** - Postman測試集合，包含所有API的測試請求範例
- 文檔內容涵蓋：
  - 5個核心API Controller的完整端點說明
  - 統一ApiResponse格式規範
  - 詳細的請求/回應JSON範例
  - 資料模型與驗證規則說明
  - HTTP狀態碼與錯誤處理機制
  - curl測試指令範例
  - Postman Collection for 便於API測試

### 2025/08/12 - 整合測試完成
- 完成API端點整合測試：
  - 使用 curl 指令測試所有主要 API 端點
  - 驗證 GET、POST、PUT、DELETE 操作正常運作
  - 測試錯誤處理機制（重複使用者名稱、資料驗證等）
  - 修正 JsonDataService 枚舉轉換問題
  - 所有 API 端點回應格式統一，符合 ApiResponse 規範
- 測試結果總結：
  - ✅ Users API - 完整 CRUD 操作正常
  - ✅ PersonalProfiles API - 資料讀取正常
  - ✅ Educations API - 排序功能正常
  - ✅ WorkExperiences API - 資料結構完整
  - ✅ Skills API - 枚舉轉換修正後正常運作
  - ✅ 錯誤處理機制完善
  - ✅ 資料驗證功能正常

### 2025/08/12 - API接口開發完成
- 建立完整的API接口架構：
  - UsersController - 使用者管理，支援CRUD操作與重複檢查
  - PersonalProfilesController - 個人資料管理，支援公開/私人設定
  - EducationsController - 學歷管理，支援排序與公開篩選
  - WorkExperiencesController - 工作經歷，支援目前職位查詢
  - SkillsController - 技能管理，支援分類與等級篩選
- 建立ApiResponse統一回應格式，支援泛型與非泛型版本
- 設定JsonDataService依賴注入，整合JSON資料存取
- 完成API基礎測試，確認服務正常運作
- 實作完整的錯誤處理與資料驗證機制

### 2025/08/12 - Models與資料模擬完成
- 建立12個完整的Model類別，包含：
  - User, PersonalProfile, Education, WorkExperience
  - Skill, Portfolio, CalendarEvent, WorkTask, TodoItem
  - BlogPost, GuestBookEntry, ContactMethod
- 建立JSON模擬資料檔案，包含豐富的測試資料
- 開發JsonDataService統一資料存取服務
- 設定完整的導航屬性與資料驗證
- 定義適當的Enum類型 (SkillLevel, TaskStatus等)

### 2025/08/08 - 後端專案初始化
- 建立基本專案結構
- 安裝必要的NuGet套件
- 設定基本的ApplicationDbContext
- 建立BaseController基礎類別
- 設定Swagger文件產生
- 設定CORS政策

## 注意事項

- 每個API都需要適當的錯誤處理
- 敏感資料不可記錄在Log中
- 資料庫密碼等敏感資訊使用User Secrets或環境變數
- 所有API都需要撰寫對應的單元測試
- 遵循RESTful API設計原則
- 開發時注意擴充性，保持良好的模組化結構