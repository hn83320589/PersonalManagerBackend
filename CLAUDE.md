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
- [ ] 設計資料庫架構 (ERD)
- [ ] 建立資料庫 Migration 檔案
- [ ] 建立 User 模型與相關設定
- [ ] 建立 Profile 模型 (個人介紹)
- [ ] 建立 Experience 模型 (學經歷)
- [ ] 建立 Skill 模型 (專長)
- [ ] 建立 Project 模型 (作品)
- [ ] 建立 Calendar 模型 (行事曆)
- [ ] 建立 Task 模型 (待辦事項)
- [ ] 建立 WorkTracking 模型 (工作追蹤)
- [ ] 建立 BlogPost 模型 (文章/網誌)
- [ ] 建立 Comment 模型 (留言)
- [ ] 建立 Contact 模型 (聯絡資訊)
- [ ] 設定模型間的關聯關係
- [ ] 建立資料庫種子資料 (Seed Data)

### API開發
- [ ] 建立 AuthController (身份驗證)
- [ ] 建立 ProfileController (個人資料)
- [ ] 建立 ExperienceController (學經歷)
- [ ] 建立 SkillController (專長)
- [ ] 建立 ProjectController (作品)
- [ ] 建立 CalendarController (行事曆)
- [ ] 建立 TaskController (待辦事項)
- [ ] 建立 WorkTrackingController (工作追蹤)
- [ ] 建立 BlogController (文章/網誌)
- [ ] 建立 CommentController (留言)
- [ ] 建立 ContactController (聯絡資訊)

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
- [ ] 設定單元測試專案
- [ ] 撰寫 Controller 單元測試
- [ ] 撰寫 Service 單元測試
- [ ] 撰寫整合測試
- [ ] 設定測試資料庫

### 部署準備
- [ ] 設定 Docker 容器化
- [ ] 準備 Production 環境設定
- [ ] 建立 CI/CD 管線設定
- [ ] 撰寫 API 文件

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