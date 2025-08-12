# Personal Manager Database

## 資料庫設計說明

此資料庫設計支援 Personal Manager 系統的所有功能模組，採用 MariaDB/MySQL 資料庫。

## 檔案說明

- `00_CreateDatabase.sql` - 建立資料庫
- `01_CreateTables.sql` - 建立所有資料表結構
- `02_CreateIndexes.sql` - 建立索引提升查詢效能
- `03_InsertSampleData.sql` - 插入範例測試資料

## 資料表結構

### 核心表格

1. **Users** - 使用者帳號管理
2. **PersonalProfiles** - 個人基本資料
3. **Educations** - 學歷資訊
4. **WorkExperiences** - 工作經歷
5. **Skills** - 專長技能
6. **Portfolios** - 作品集
7. **CalendarEvents** - 行事曆事件
8. **WorkTasks** - 工作任務追蹤
9. **TodoItems** - 個人待辦事項
10. **BlogPosts** - 部落格文章
11. **GuestBookEntries** - 留言板
12. **ContactMethods** - 聯絡方式

## 資料庫初始化步驟

1. 確保 MariaDB/MySQL 服務正在運行
2. 執行 `00_CreateDatabase.sql` 建立資料庫
3. 執行 `01_CreateTables.sql` 建立資料表
4. 執行 `02_CreateIndexes.sql` 建立索引
5. (可選) 執行 `03_InsertSampleData.sql` 插入測試資料

## 重要特性

- **UTF-8 編碼**: 支援多國語言字元
- **外鍵約束**: 確保資料完整性
- **軟刪除**: 重要資料採用 IsActive 欄位標記而非實際刪除
- **時間戳記**: 所有表格都有 CreatedAt 和 UpdatedAt 欄位
- **公開/私人**: 支援內容的可見性控制
- **排序**: 支援自訂排序順序

## 索引策略

針對常用查詢建立複合索引：
- 使用者相關查詢 (UserId)
- 公開內容查詢 (IsPublic)
- 日期範圍查詢 (StartDate, EndDate)
- 狀態篩選查詢 (Status, Priority)

## 安全考量

- 密碼採用雜湊儲存
- 支援 IP 位址記錄
- 留言板內容需審核機制
- 個人資料支援隱私控制