# Personal Manager Backend - Production Dockerfile
# 支援 Zeabur 部署的 .NET 8.0 Linux 容器

# Build Stage - 建置階段
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# 複製專案檔案並還原依賴
COPY ["code/PersonalManagerAPI.csproj", "."]
RUN dotnet restore "PersonalManagerAPI.csproj" --runtime linux-x64

# 複製所有原始碼並建置
COPY code/ .
RUN dotnet build "PersonalManagerAPI.csproj" -c Release -o /app/build --runtime linux-x64

# Publish Stage - 發布階段
FROM build AS publish
RUN dotnet publish "PersonalManagerAPI.csproj" -c Release -o /app/publish \
    --runtime linux-x64 \
    --self-contained false \
    --no-restore

# Runtime Stage - 執行階段
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# 安裝必要的套件（時區、CA證書等）
RUN apk add --no-cache \
    tzdata \
    ca-certificates \
    && rm -rf /var/cache/apk/*

# 設定時區為台灣時間
ENV TZ=Asia/Taipei

# 建立非root使用者以提高安全性
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# 設定工作目錄
WORKDIR /app

# 建立必要的目錄並設定權限
RUN mkdir -p /app/Data/JsonData /app/uploads /app/quarantine && \
    chown -R appuser:appgroup /app

# 複製發布的應用程式
COPY --from=publish /app/publish .
COPY --chown=appuser:appgroup Data/JsonData/ ./Data/JsonData/

# 設定檔案權限
RUN chown -R appuser:appgroup /app && \
    chmod -R 755 /app

# 切換到非root使用者
USER appuser

# 設定環境變數
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# 暴露連接埠
EXPOSE 8080

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# 應用程式入口點
ENTRYPOINT ["dotnet", "PersonalManagerAPI.dll"]