# Personal Manager Backend - Docker Configuration

這個目錄包含 Personal Manager Backend 的 Docker 配置檔案。

## 檔案說明

### 核心檔案
- **Dockerfile** - 生產環境 Docker 映像建置配置
- **docker-compose.yml** - API 服務配置（連接外部 Zeabur DB）
- **zeabur.yml** - Zeabur 平台部署配置
- **.dockerignore** - Docker 建置忽略檔案

## 目錄結構

```
PersonalManagerBackend/
├── docker/                 # Docker 配置檔案
│   ├── Dockerfile          # Docker 映像建置檔
│   ├── docker-compose.yml  # 服務編排檔
│   ├── zeabur.yml          # Zeabur 部署檔
│   ├── .dockerignore       # Docker 忽略檔
│   └── README.md           # 本檔案
└── code/                   # 原始碼目錄
    ├── Controllers/        # API 控制器
    ├── Models/            # 資料模型
    ├── Services/          # 業務邏輯服務
    ├── Data/              # 資料存取層
    └── ...                # 其他原始碼檔案
```

## 使用方法

### 1. 本地開發
從 docker/ 目錄執行：

```bash
# 進入 docker 目錄
cd docker

# 設定資料庫連接字串環境變數（如果有的話）
export DATABASE_CONNECTION_STRING="your_zeabur_db_connection_string"

# 啟動 API 服務（連接外部 Zeabur DB）
docker-compose up personalmanager-api

# 背景執行
docker-compose up -d personalmanager-api
```

### 2. 生產環境建置
```bash
cd docker
docker build -t personalmanager-backend -f Dockerfile ..
```

### 3. Zeabur 部署
將整個專案推送到 Git 倉庫，Zeabur 會自動識別 `zeabur.yml` 配置並進行部署。

## 環境變數

### 必需的環境變數
- `JWT_SECRET_KEY` - JWT 簽名密鑰（最少32字元）
- `DATABASE_CONNECTION_STRING` - Zeabur DB Server 連接字串

### 可選的環境變數  
- `JWT_ISSUER` - JWT 發行者 (預設: PersonalManagerAPI)
- `JWT_AUDIENCE` - JWT 接收者 (預設: PersonalManagerClient)
- `JWT_EXPIRY_MINUTES` - JWT 過期時間分鐘 (預設: 60)
- `JWT_REFRESH_EXPIRY_DAYS` - Refresh Token 過期天數 (預設: 7)

## 健康檢查

API 服務配置了健康檢查：
- **API**: `GET /health` - 檢查服務運行狀態

## 注意事項

1. **外部資料庫**: 此配置針對使用 Zeabur DB Server，不包含本地資料庫
2. **路徑配置**: 所有 Docker 檔案中的路徑都已調整為相對於新的目錄結構  
3. **建置上下文**: Docker 建置的上下文是父目錄 (`..`)，以便存取 `code/` 目錄
4. **資料持久化**: 檔案上傳、日誌等應用程式資料有適當的 volume 配置
5. **安全性**: 使用非 root 使用者執行，並配置適當的檔案權限

## 故障排除

### 常見問題

1. **路徑錯誤**
   - 確保在 `docker/` 目錄中執行命令
   - 檢查相對路徑配置是否正確

2. **權限問題**
   - 確保 Docker 有權限存取所需的目錄
   - 檢查檔案所有權設定

3. **連接埠衝突**
   - 預設連接埠：API (8080)
   - 可在 `docker-compose.yml` 中修改連接埠對應

4. **資料庫連接問題**
   - 確保 `DATABASE_CONNECTION_STRING` 環境變數正確設定
   - 檢查 Zeabur DB Server 的連接字串格式

5. **記憶體不足**
   - 調整 `docker-compose.yml` 中的資源限制
   - 確保系統有足夠的可用記憶體