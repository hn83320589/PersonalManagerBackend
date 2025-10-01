# Personal Manager Backend CI/CD 指南

## 🚀 CI/CD 管線概覽

Personal Manager Backend 專案使用 GitHub Actions 實現完整的 CI/CD 管線，包含建置、測試、安全掃描、效能測試和自動部署。

## 📁 CI/CD 檔案結構

```
.github/
├── workflows/
│   ├── ci-cd.yml              # 主要 CI/CD 管線
│   └── dependency-update.yml  # 依賴更新和安全檢查
├── codeql/
│   └── codeql-config.yml      # CodeQL 安全掃描配置
tests/
└── performance/
    └── api-load-test.js       # K6 效能測試腳本
sonar-project.properties       # SonarCloud 程式碼品質配置
.env.example                   # 環境變數範本
```

## 🔄 工作流程說明

### 主要 CI/CD 管線 (`ci-cd.yml`)

#### 1. 程式碼分析與安全掃描 (`code-analysis`)
- **SonarCloud 分析**: 程式碼品質、安全漏洞、代碼覆蓋率
- **CodeQL 分析**: GitHub 進階安全漏洞掃描
- **依賴安全檢查**: 檢查過時和有漏洞的 NuGet 套件
- **觸發條件**: 所有推送和 PR

#### 2. 建置與測試 (`build-and-test`)
- **多配置建置**: Debug 和 Release 配置
- **單元測試**: 包含測試報告和代碼覆蓋率
- **NuGet 快取**: 加速建置過程
- **測試報告**: 自動生成和上傳測試結果

#### 3. Docker 建置與安全掃描 (`docker-build`)
- **多平台建置**: Linux AMD64 和 ARM64
- **容器登錄**: 推送到 GitHub Container Registry
- **Trivy 安全掃描**: 容器映像漏洞掃描
- **元資料標籤**: 自動版本標記和標籤

#### 4. 效能測試 (`performance-test`)
- **K6 負載測試**: API 效能和負載測試
- **Redis 服務**: 整合 Redis 進行實際測試
- **效能指標**: 回應時間、錯誤率、吞吐量
- **僅在 main 分支執行**

#### 5. 整合測試 (`integration-test`)
- **PostgreSQL 服務**: 真實資料庫環境測試
- **EF 遷移**: 自動執行資料庫遷移
- **端到端測試**: 完整應用程式流程測試

#### 6. 部署流程
- **Staging 部署**: 自動部署 develop 分支到測試環境
- **Production 部署**: 自動部署 main 分支到生產環境
- **健康檢查**: 部署後自動驗證服務狀態
- **通知機制**: Slack 部署狀態通知

### 依賴更新工作流程 (`dependency-update.yml`)

#### 1. 自動依賴更新 (`update-dependencies`)
- **每週執行**: 每週一自動檢查更新
- **NuGet 套件更新**: 自動更新到最新相容版本
- **安全性檢查**: 檢查已知漏洞
- **自動 PR**: 建立更新 Pull Request

#### 2. 安全稽核 (`security-audit`)
- **漏洞掃描**: 檢查已知安全漏洞
- **過時套件檢查**: 識別不再維護的套件
- **自動警報**: 發現問題時建立 Issue

#### 3. 授權合規檢查 (`license-check`)
- **授權檢查**: 驗證所有依賴的授權合規性
- **報告生成**: 生成詳細授權報告

## 🔧 設置指南

### 1. 必要的 GitHub Secrets

在 GitHub 倉庫設定中添加以下 Secrets：

```bash
# SonarCloud 整合
SONAR_TOKEN=your-sonar-token

# Zeabur 部署
ZEABUR_API_TOKEN=your-zeabur-api-token
ZEABUR_STAGING_SERVICE_ID=your-staging-service-id
ZEABUR_PRODUCTION_SERVICE_ID=your-production-service-id

# 通知服務
SLACK_WEBHOOK=your-slack-webhook-url
MONITORING_WEBHOOK=your-monitoring-webhook-url

# 程式碼覆蓋率
CODECOV_TOKEN=your-codecov-token
```

### 2. SonarCloud 設置

1. 在 [SonarCloud.io](https://sonarcloud.io) 建立專案
2. 設定專案金鑰: `personal-manager-backend`
3. 將 `SONAR_TOKEN` 添加到 GitHub Secrets
4. 配置 Quality Gate 規則

### 3. Zeabur 部署設置

1. 在 Zeabur 建立專案和服務
2. 設定環境變數
3. 配置資料庫連線
4. 將服務 ID 和 API Token 添加到 Secrets

### 4. 通知設置

#### Slack 整合
1. 建立 Slack Incoming Webhook
2. 設定適當的頻道 (#deployments, #production)
3. 將 Webhook URL 添加到 Secrets

## 📊 品質門檻

### 程式碼品質要求
- **單元測試覆蓋率**: > 80%
- **SonarCloud Quality Gate**: Pass
- **安全漏洞**: 0 個高危漏洞
- **程式碼重複率**: < 3%

### 效能要求
- **API 回應時間**: 95% 請求 < 500ms
- **錯誤率**: < 10%
- **Docker 映像大小**: < 200MB (優化目標)

### 安全要求
- **依賴漏洞**: 0 個已知高危漏洞
- **Container 安全**: Trivy 掃描通過
- **CodeQL 分析**: 無安全警告

## 🚦 分支策略

### 主要分支
- **`main`**: 生產環境分支，觸發完整 CI/CD 管線
- **`develop`**: 開發環境分支，觸發建置和測試環境部署
- **`feature/*`**: 功能開發分支，觸發建置和測試

### 保護規則
- main 分支需要 PR 審查
- 必須通過所有檢查才能合併
- 不允許直接推送到 main

## 📈 監控與警報

### 建置監控
- **GitHub Actions**: 即時建置狀態
- **測試報告**: 自動生成和歷史追蹤
- **程式碼覆蓋率**: Codecov 整合

### 部署監控
- **健康檢查**: 自動驗證部署成功
- **效能監控**: 部署後效能驗證
- **錯誤追蹤**: 自動錯誤檢測和警報

### 安全監控
- **漏洞掃描**: 每週自動掃描
- **依賴更新**: 自動更新和安全檢查
- **容器安全**: 每次建置安全掃描

## 🔍 故障排除

### 常見問題

#### 1. 建置失敗
```bash
# 檢查依賴
dotnet restore
dotnet build

# 檢查測試
dotnet test
```

#### 2. Docker 建置失敗
```bash
# 本地測試
docker build -f docker/Dockerfile .

# 檢查 Dockerfile 語法
docker build --dry-run -f docker/Dockerfile .
```

#### 3. 部署失敗
- 檢查 Zeabur 服務狀態
- 驗證環境變數設定
- 檢查資料庫連線

#### 4. 測試失敗
- 檢查測試環境配置
- 驗證資料庫連線
- 檢查測試資料

### 除錯模式

啟用詳細日誌：
```yaml
- name: Enable debug logging
  run: echo "ACTIONS_STEP_DEBUG=true" >> $GITHUB_ENV
```

## 📝 最佳實踐

### 1. 提交規範
```
feat: 新增功能
fix: 修復問題
docs: 文檔更新
style: 代碼格式
refactor: 重構
test: 測試相關
chore: 建置或工具變更
```

### 2. PR 檢查清單
- [ ] 所有測試通過
- [ ] 程式碼覆蓋率符合要求
- [ ] SonarCloud 品質門檻通過
- [ ] 安全掃描無高危漏洞
- [ ] 文檔已更新
- [ ] CHANGELOG 已更新

### 3. 發布流程
1. 從 develop 建立 release 分支
2. 完成功能測試和品質檢查
3. 合併到 main 分支
4. 標記版本標籤
5. 自動部署到生產環境

## 🎯 未來改進

### 短期目標
- [ ] 增加端到端測試
- [ ] 整合更多安全掃描工具
- [ ] 增加效能基準測試
- [ ] 實作藍綠部署

### 長期目標
- [ ] 實作 GitOps 工作流程
- [ ] 增加多環境支援
- [ ] 實作自動回滾機制
- [ ] 建立監控儀表板

---

## 💡 提示

這個 CI/CD 管線為企業級專案設計，包含完整的品質保證、安全掃描和自動化部署。根據專案需求，您可以調整配置以適應特定要求。