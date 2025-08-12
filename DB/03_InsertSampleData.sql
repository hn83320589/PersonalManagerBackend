-- Personal Manager Sample Data Script
-- Created: 2025-08-12
-- Description: 插入範例資料用於測試

USE `PersonalManager`;

-- 插入範例使用者 (密碼: password123，需要在應用程式中進行雜湊)
INSERT INTO `Users` (`Username`, `Email`, `PasswordHash`, `FirstName`, `LastName`, `Phone`, `IsActive`) VALUES
('admin', 'admin@example.com', '$2a$10$example_hash_here', '管理員', '使用者', '0912345678', 1),
('john_doe', 'john.doe@example.com', '$2a$10$example_hash_here', 'John', 'Doe', '0987654321', 1);

-- 插入個人基本資料
INSERT INTO `PersonalProfiles` (`UserId`, `Title`, `Summary`, `Description`, `Website`, `Location`, `IsPublic`) VALUES
(1, '全端開發工程師', '專精於 .NET Core 與 Vue.js 開發', '擁有5年以上全端開發經驗，熟悉現代化網頁開發技術棧...', 'https://example.com', '台灣 台北', 1);

-- 插入學歷資料
INSERT INTO `Educations` (`UserId`, `School`, `Degree`, `FieldOfStudy`, `StartDate`, `EndDate`, `Description`, `IsPublic`, `SortOrder`) VALUES
(1, '國立台灣大學', '碩士', '資訊工程學系', '2015-09-01', '2017-06-30', '主修軟體工程與資料庫系統', 1, 1),
(1, '國立交通大學', '學士', '資訊工程學系', '2011-09-01', '2015-06-30', '主修程式設計與演算法', 1, 2);

-- 插入工作經歷
INSERT INTO `WorkExperiences` (`UserId`, `Company`, `Position`, `StartDate`, `EndDate`, `IsCurrent`, `Description`, `IsPublic`, `SortOrder`) VALUES
(1, 'ABC科技公司', '資深全端工程師', '2020-03-01', NULL, 1, '負責企業級應用系統開發與維護', 1, 1),
(1, 'XYZ軟體公司', '後端工程師', '2017-07-01', '2020-02-28', 0, '開發RESTful API與資料庫設計', 1, 2);

-- 插入技能資料
INSERT INTO `Skills` (`UserId`, `Name`, `Category`, `Level`, `Description`, `IsPublic`, `SortOrder`) VALUES
(1, 'C# .NET Core', '後端開發', 'Expert', '5年以上開發經驗', 1, 1),
(1, 'Vue.js', '前端開發', 'Advanced', '3年以上開發經驗', 1, 2),
(1, 'MariaDB/MySQL', '資料庫', 'Advanced', '資料庫設計與優化', 1, 3),
(1, 'Docker', 'DevOps', 'Intermediate', '容器化部署經驗', 1, 4);

-- 插入作品集
INSERT INTO `Portfolios` (`UserId`, `Title`, `Description`, `TechnologyUsed`, `ProjectUrl`, `GithubUrl`, `StartDate`, `EndDate`, `IsPublic`, `IsFeatured`, `SortOrder`) VALUES
(1, 'Personal Manager 系統', '個人服務管理系統，包含履歷、待辦事項、行事曆等功能', '.NET Core, Vue.js, MariaDB', 'https://personalmanager.example.com', 'https://github.com/example/personal-manager', '2025-08-01', NULL, 1, 1, 1),
(1, 'E-Commerce API', '電商平台後端API系統', '.NET Core, Entity Framework, Redis', NULL, 'https://github.com/example/ecommerce-api', '2024-01-01', '2024-06-30', 1, 0, 2);

-- 插入行事曆事件 (範例)
INSERT INTO `CalendarEvents` (`UserId`, `Title`, `Description`, `StartDate`, `EndDate`, `IsAllDay`, `IsPublic`, `EventType`) VALUES
(1, '專案會議', '討論 Personal Manager 系統需求', '2025-08-15 10:00:00', '2025-08-15 11:30:00', 0, 0, 'Meeting'),
(1, '技術研討會', '參加 .NET Conf 2025', '2025-09-01 09:00:00', '2025-09-01 17:00:00', 1, 1, 'Other');

-- 插入工作任務
INSERT INTO `WorkTasks` (`UserId`, `Title`, `Description`, `Status`, `Priority`, `StartDate`, `DueDate`, `EstimatedHours`, `Project`) VALUES
(1, '完成資料庫設計', '設計Personal Manager系統的資料庫結構', 'InProgress', 'High', '2025-08-12 09:00:00', '2025-08-13 18:00:00', 8.0, 'Personal Manager'),
(1, '開發使用者認證API', '實作JWT token認證機制', 'Planning', 'High', '2025-08-14 09:00:00', '2025-08-16 18:00:00', 16.0, 'Personal Manager');

-- 插入待辦事項
INSERT INTO `TodoItems` (`UserId`, `Title`, `Description`, `IsCompleted`, `Priority`, `DueDate`, `Category`) VALUES
(1, '完成資料庫設計文檔', '撰寫詳細的資料庫設計說明文檔', 0, 'High', '2025-08-13 18:00:00', '工作'),
(1, '購買開發用伺服器', '評估並購買適合的雲端伺服器方案', 0, 'Medium', '2025-08-20 12:00:00', '採購'),
(1, '學習新技術', '研讀 Docker Kubernetes 相關文檔', 0, 'Low', '2025-08-30 23:59:59', '學習');

-- 插入部落格文章
INSERT INTO `BlogPosts` (`UserId`, `Title`, `Content`, `Summary`, `IsPublished`, `IsPublic`, `PublishedDate`, `Category`, `Tags`) VALUES
(1, '如何設計可擴展的資料庫架構', '在現代應用程式開發中，資料庫設計是非常重要的一環...', '分享資料庫架構設計的經驗與最佳實踐', 1, 1, '2025-08-10 10:00:00', '技術分享', '資料庫,架構設計,.NET Core'),
(1, 'Vue.js 3 與 TypeScript 開發心得', 'Vue.js 3 結合 TypeScript 可以大幅提升開發效率...', '分享 Vue3 + TypeScript 的開發經驗', 0, 0, NULL, '前端開發', 'Vue.js,TypeScript,前端');

-- 插入留言板範例
INSERT INTO `GuestBookEntries` (`Name`, `Email`, `Website`, `Message`, `IsApproved`, `IsPublic`) VALUES
('訪客A', 'visitor@example.com', 'https://visitor-blog.com', '很棒的個人網站！期待看到更多技術分享。', 1, 1),
('John Smith', 'john@example.com', NULL, '感謝分享這些實用的開發經驗，對我很有幫助！', 1, 1);

-- 插入聯絡方式
INSERT INTO `ContactMethods` (`UserId`, `Type`, `Value`, `Label`, `IsPublic`, `IsPreferred`, `SortOrder`) VALUES
(1, 'Email', 'admin@example.com', '工作信箱', 1, 1, 1),
(1, 'Phone', '0912345678', '手機', 1, 0, 2),
(1, 'GitHub', 'https://github.com/example', 'GitHub Profile', 1, 0, 3),
(1, 'LinkedIn', 'https://linkedin.com/in/example', 'LinkedIn Profile', 1, 0, 4);