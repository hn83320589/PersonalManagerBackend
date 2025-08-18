# Personal Manager Backend

這是Personal Manager系統的後端API專案，使用C# .NET Core Web API開發，具備企業級JWT認證、檔案安全驗證、統一錯誤處理等功能。

## 🚀 快速開始

### 前置需求

- .NET 9.0 SDK
- Visual Studio Code 或 Visual Studio
- Docker (可選，用於容器化部署)
- Zeabur DB Server 連接字串 (生產環境)

### 專案結構說明

此專案採用**分離配置架構**，將 Docker 配置與原始碼分開管理：

```
PersonalManagerBackend/
├── docker/                    # Docker 配置目錄
│   ├── Dockerfile            # Docker 映像建置檔
│   ├── docker-compose.yml    # 服務編排檔 (僅API服務)
│   ├── zeabur.yml            # Zeabur 部署配置
│   └── README.md             # Docker 使用說明
└── code/                     # 原始碼目錄
    ├── Controllers/          # API控制器 (13個)
    ├── Models/               # 資料模型 (12個)
    ├── Services/             # 業務邏輯服務
    ├── Middleware/           # 企業級中介軟體
    ├── Data/                 # 資料存取層
    └── ...                   # 其他原始碼檔案
```

### 本地開發安裝與執行

1. **Clone 專案**
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerBackend.git
   cd PersonalManagerBackend
   ```

2. **進入原始碼目錄**
   ```bash
   cd code
   ```

3. **還原套件**
   ```bash
   dotnet restore
   ```

4. **資料存取設定**
   - 目前使用 JSON 模擬資料，無需資料庫設定
   - JSON 資料檔案位於 `Data/JsonData/` 目錄
   - 生產環境可設定 `DATABASE_CONNECTION_STRING` 連接 Zeabur DB Server

5. **執行專案**
   ```bash
   dotnet run
   ```

6. **存取 API**
   - API 基礎路徑: `http://localhost:5253/api`
   - Swagger 文檔: `http://localhost:5253/swagger`
   - 如果埠口衝突，可使用: `dotnet run --urls "http://localhost:5253"`

### Docker 容器化部署

```bash
# 進入 Docker 配置目錄
cd docker

# 建置 Docker 映像
docker build -t personalmanager-backend -f Dockerfile ..

# 設定環境變數 (可選)
export DATABASE_CONNECTION_STRING="your_zeabur_db_connection_string"
export JWT_SECRET_KEY="your_jwt_secret_key"

# 啟動 API 服務
docker-compose up personalmanager-api
```

## 🛠️ 技術架構

### 核心技術棧
- **框架**: ASP.NET Core 9.0 Web API
- **資料存取**: JsonDataService (開發) + Entity Framework Core 9.0.8 (生產)
- **資料庫**: JSON 模擬資料 + Zeabur MariaDB Server (Pomelo.EntityFrameworkCore.MySql 9.0.0)
- **認證**: JWT Bearer Authentication + BCrypt 密碼雜湊
- **文檔**: Swagger/OpenAPI + 完整 API 文檔 + Postman Collection
- **測試**: xUnit 基礎測試 + 手動整合測試 + Playwright E2E
- **部署**: Docker 容器化 + Zeabur 平台部署

### 企業級功能
- **JWT 認證系統**: 完整的使用者認證與授權機制
- **統一錯誤處理**: ErrorHandlingMiddleware 全域例外捕獲
- **請求日誌記錄**: RequestLoggingMiddleware 完整追蹤
- **檔案安全驗證**: 多層檔案安全檢查與惡意檔案隔離
- **統一回應格式**: ApiResponse 標準化所有 API 回應

## 📁 原始碼結構 (code/ 目錄)

```
code/
├── Controllers/            # API 控制器 (13個)
│   ├── AuthController.cs         # JWT 身份驗證 API (7個端點)
│   ├── BaseController.cs         # 控制器基礎類別
│   ├── UsersController.cs        # 使用者管理 API (5個端點)
│   ├── PersonalProfilesController.cs  # 個人資料 API (6個端點)
│   ├── EducationsController.cs   # 學歷管理 API (6個端點)
│   ├── WorkExperiencesController.cs   # 工作經歷 API (7個端點)
│   ├── SkillsController.cs       # 技能管理 API (8個端點)
│   ├── PortfoliosController.cs   # 作品集 API (7個端點)
│   ├── CalendarEventsController.cs    # 行事曆 API (6個端點)
│   ├── TodoItemsController.cs    # 待辦事項 API (6個端點)
│   ├── WorkTasksController.cs    # 工作追蹤 API (5個端點)
│   ├── BlogPostsController.cs    # 部落格 API (5個端點)
│   ├── GuestBookEntriesController.cs  # 留言板 API (4個端點)
│   ├── ContactMethodsController.cs    # 聯絡資訊 API (3個端點)
│   └── FilesController.cs        # 檔案上傳 API (3個端點)
├── Models/                 # 資料模型 (12個)
│   ├── User.cs                   # 使用者模型 (含JWT欄位)
│   ├── PersonalProfile.cs        # 個人資料模型
│   ├── Education.cs, WorkExperience.cs, Skill.cs, Portfolio.cs
│   ├── CalendarEvent.cs, TodoItem.cs, WorkTask.cs
│   ├── BlogPost.cs, GuestBookEntry.cs, ContactMethod.cs
│   └── FileUpload.cs             # 檔案上傳模型
├── Services/               # 業務邏輯服務
│   ├── AuthService.cs            # JWT 認證服務 (BCrypt + Token管理)
│   ├── JsonDataService.cs        # JSON 資料存取服務
│   ├── FileService.cs            # 檔案管理服務
│   ├── FileSecurityService.cs    # 檔案安全驗證服務
│   └── FileQuarantineService.cs  # 檔案隔離服務
├── DTOs/                   # 資料傳輸物件
│   ├── ApiResponse.cs            # 統一 API 回應格式
│   ├── LoginDto.cs, RegisterDto.cs, TokenResponseDto.cs
│   └── FileUploadDto.cs          # 檔案上傳 DTO
├── Middleware/             # 企業級中介軟體
│   ├── ErrorHandlingMiddleware.cs     # 統一錯誤處理
│   ├── RequestLoggingMiddleware.cs    # 請求日誌記錄
│   ├── MiddlewareExtensions.cs        # 中介軟體擴展
│   └── Exceptions/                    # 自訂例外類別
├── Data/                   # 資料存取層
│   ├── ApplicationDbContext.cs   # EF Core 資料庫上下文
│   └── JsonData/                 # JSON 模擬資料 (13個檔案)
├── DB/                     # 資料庫設計
│   ├── 00_CreateDatabase.sql     # 資料庫建立腳本
│   ├── 01_CreateTables.sql       # 資料表建立腳本
│   ├── 02_CreateIndexes.sql      # 索引建立腳本
│   └── 03_InsertSampleData.sql   # 範例資料插入腳本
├── wwwroot/                # 靜態資源與檔案上傳
├── PersonalManagerAPI.csproj     # 專案檔案
├── Program.cs                    # 程式進入點 (完整中介軟體管線)
└── appsettings.json              # 應用程式設定
```

## 🔧 開發指令

### 本地開發 (進入 code/ 目錄)
```bash
cd code

# 還原套件
dotnet restore

# 建置專案
dotnet build

# 執行專案 (開發模式)
dotnet run

# 執行測試
dotnet test

# 新增套件
dotnet add package PackageName
```

### Docker 部署 (進入 docker/ 目錄)
```bash
cd docker

# 建置 Docker 映像
docker build -t personalmanager-backend -f Dockerfile ..

# 啟動 API 服務
docker-compose up personalmanager-api

# 背景執行
docker-compose up -d personalmanager-api

# 停止服務
docker-compose down
```

### Entity Framework (未來資料庫遷移)
```bash
cd code

# 建立 Migration
dotnet ef migrations add InitialCreate

# 更新資料庫
dotnet ef database update

# 移除最後一個 Migration
dotnet ef migrations remove
```

## 🌟 主要功能

### ✅ 第一期已完成 (100%)

#### 核心系統架構
- [x] **企業級 API 架構**: ASP.NET Core 9.0 + 中介軟體管線
- [x] **JWT 認證系統**: 完整使用者認證與授權 (AuthController - 7個端點)
- [x] **檔案安全系統**: 多層檔案驗證與惡意檔案隔離
- [x] **統一錯誤處理**: ErrorHandlingMiddleware + 自訂例外類別
- [x] **請求日誌記錄**: RequestLoggingMiddleware + 完整追蹤
- [x] **JsonDataService**: 完整的JSON模擬資料管理
- [x] **統一回應格式**: ApiResponse 標準化所有回應

#### API Controllers (13個完成)
- [x] **AuthController**: JWT 認證 API - 7個端點 (登入/註冊/Token管理)
- [x] **UsersController**: 使用者管理 API - 5個端點
- [x] **PersonalProfilesController**: 個人資料 API - 6個端點
- [x] **EducationsController**: 學歷管理 API - 6個端點
- [x] **WorkExperiencesController**: 工作經歷 API - 7個端點
- [x] **SkillsController**: 技能管理 API - 8個端點
- [x] **PortfoliosController**: 作品集 API - 7個端點
- [x] **CalendarEventsController**: 行事曆 API - 6個端點
- [x] **TodoItemsController**: 待辦事項 API - 6個端點
- [x] **WorkTasksController**: 工作追蹤 API - 5個端點
- [x] **BlogPostsController**: 部落格 API - 5個端點
- [x] **GuestBookEntriesController**: 留言板 API - 4個端點
- [x] **ContactMethodsController**: 聯絡資訊 API - 3個端點
- [x] **FilesController**: 檔案上傳 API - 3個端點

#### 品質保證與部署
- [x] **API 文檔系統**: Swagger + 完整技術文檔 + Postman Collection
- [x] **整合測試**: 手動API測試完成，所有端點驗證通過
- [x] **前後端整合**: CORS配置完成，API整合100%成功
- [x] **Docker 容器化**: 完整 Docker 配置，支援 Zeabur 部署
- [x] **企業級安全**: 檔案安全驗證、JWT認證、錯誤處理

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

#### 本地開發
```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5253
```

#### 生產環境 (Docker/Zeabur)
```env
# 必填環境變數
JWT_SECRET_KEY=your_jwt_secret_key_minimum_32_characters
DATABASE_CONNECTION_STRING=your_zeabur_db_connection_string

# 可選環境變數
JWT_ISSUER=PersonalManagerAPI
JWT_AUDIENCE=PersonalManagerClient
JWT_EXPIRY_MINUTES=60
JWT_REFRESH_EXPIRY_DAYS=7
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
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
  },
  "JwtSettings": {
    "SecretKey": "PersonalManager_SuperSecretKey_2025_MinimumLength32Characters",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Zeabur 部署環境變數設定

在 Zeabur 控制台設定以下環境變數：

1. **JWT_SECRET_KEY** (必填) - JWT 簽名密鑰，至少32字元
2. **DATABASE_CONNECTION_STRING** (必填) - Zeabur DB Server 連接字串
3. **FRONTEND_URL** (可選) - 前端應用程式 URL，用於 CORS 設定

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

#### 本地 Docker 測試
```bash
cd docker

# 建置映像
docker build -t personalmanager-backend -f Dockerfile ..

# 設定環境變數
export JWT_SECRET_KEY="your_jwt_secret_key"
export DATABASE_CONNECTION_STRING="your_db_connection"

# 啟動容器
docker-compose up personalmanager-api
```

### Zeabur 平台部署

#### 自動部署流程
1. **連接 Git 倉庫**: 將 PersonalManagerBackend 倉庫連接到 Zeabur
2. **設定環境變數**: 
   - `JWT_SECRET_KEY` (必填)
   - `DATABASE_CONNECTION_STRING` (必填)
   - `FRONTEND_URL` (可選)
3. **自動識別配置**: Zeabur 會自動讀取 `zeabur.yml` 配置
4. **自動建置部署**: 推送代碼後自動觸發部署

#### Zeabur 部署特色
- ✅ **自動容器化**: 基於 Dockerfile 自動建置
- ✅ **環境變數管理**: 安全的密鑰管理
- ✅ **健康檢查**: 自動監控 API 服務狀態
- ✅ **自動擴展**: 根據負載自動調整資源
- ✅ **HTTPS 支援**: 自動 SSL 憑證與 HTTPS 重導向

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