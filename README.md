# Personal Manager Backend

個人展示與管理平台後端 API，使用 .NET 9.0 Web API 開發。

## 如何執行

### 前置需求

- .NET 9.0 SDK

### 開發模式

```bash
dotnet run
```

- API：`http://localhost:5037`
- Swagger UI：`http://localhost:5037/swagger`（僅開發模式）
- Demo 帳號：`admin` / `demo123`

啟動時自動偵測 MariaDB 連線。連線失敗則自動切換至本地 JSON 資料（`Data/JsonData/*.json`），不影響開發。

## 設定

### appsettings.json（提交至 git，只含占位符）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "SecretKey": "CHANGE_THIS_TO_A_RANDOM_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "ExpiryHours": 24
  }
}
```

### appsettings.Development.json（gitignored，本機自行建立）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=personal_manager;User=...;Password=...;"
  },
  "Jwt": {
    "SecretKey": "your_local_secret_key_at_least_32_chars"
  }
}
```

## 專案結構

```
PersonalManagerBackend/
├── Program.cs                    # 進入點、DI 註冊、Middleware 管線
├── appsettings.json
├── Auth/                         # JWT 認證服務
├── Controllers/                  # 13 個 API Controller
├── DTOs/                         # 資料傳輸物件
├── Mappings/                     # DTO ↔ Model 映射
├── Middleware/                   # 全域錯誤處理
├── Models/                       # 12 個實體模型
├── Repositories/                 # IRepository + EF/JSON 兩種實作
├── Services/                     # 業務邏輯層（CrudService + 12 個實體服務）
└── Data/
    ├── ApplicationDbContext.cs
    ├── DatabaseSeeder.cs
    └── JsonData/                 # JSON fallback 資料檔
```

## API 端點總覽

| 前綴 | 說明 | 認證 |
|------|------|------|
| `POST /api/auth/login` | 登入，回傳 JWT | 否 |
| `POST /api/auth/register` | 註冊 | 否 |
| `GET /api/users/public` | 所有用戶公開資訊 | 否 |
| `GET /api/users/public/{username}` | 依 username 查詢用戶 | 否 |
| `GET /api/profiles/directory` | 所有 Profile + User 合併清單 | 否 |
| `GET /api/profiles/user/{userId}` | 依 userId 查詢 Profile | 否 |
| `PUT /api/profiles/{id}` | 更新 Profile | 是 |
| `GET /api/blogposts/user/{userId}/public` | 已發布文章列表 | 否 |
| `GET /api/guestbookentries/user/{targetUserId}` | 已審核留言列表 | 否 |
| `PUT /api/guestbookentries/{id}` | 審核/回覆留言 | 是 |
| `GET /api/skills/user/{userId}` | 技能列表 | 否 |
| `GET /api/portfolios/user/{userId}` | 作品集 | 否 |

> 其餘各實體均提供完整 CRUD，受保護端點需帶 `Authorization: Bearer <token>`。

## 部署（Zeabur）

在 Zeabur 環境變數中設定：

```
ConnectionStrings__DefaultConnection = <MariaDB 連線字串>
Jwt__SecretKey = <至少 32 字元的隨機密鑰>
```

推送 `main` branch 即自動部署。

> DB schema 變更時，若使用 EF 模式需 DROP 資料庫後讓 `EnsureCreated()` 重建，或執行 `ALTER TABLE`。

## 相關連結

- [主專案](https://github.com/hn83320589/personal_manager)
- [前端專案](https://github.com/hn83320589/PersonalManagerFrontend)
