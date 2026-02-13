-- Personal Manager Tables Creation Script
-- Created: 2025-08-12
-- Description: 建立所有資料表結構

USE `PersonalManager`;

-- 1. 使用者表 (支援登入功能)
CREATE TABLE `Users` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Username` varchar(50) NOT NULL UNIQUE,
    `Email` varchar(100) NOT NULL UNIQUE,
    `PasswordHash` varchar(255) NOT NULL,
    `FirstName` varchar(50) DEFAULT NULL,
    `LastName` varchar(50) DEFAULT NULL,
    `Phone` varchar(20) DEFAULT NULL,
    `IsActive` tinyint(1) DEFAULT 1,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. 個人基本資料表 (個人介紹)
CREATE TABLE `PersonalProfiles` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) DEFAULT NULL,
    `Title` varchar(100) DEFAULT NULL,
    `Summary` text DEFAULT NULL,
    `Description` longtext DEFAULT NULL,
    `ProfileImageUrl` varchar(255) DEFAULT NULL,
    `Website` varchar(255) DEFAULT NULL,
    `Location` varchar(100) DEFAULT NULL,
    `Birthday` date DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. 學歷表
CREATE TABLE `Educations` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `School` varchar(100) NOT NULL,
    `Degree` varchar(100) DEFAULT NULL,
    `FieldOfStudy` varchar(100) DEFAULT NULL,
    `StartDate` date DEFAULT NULL,
    `EndDate` date DEFAULT NULL,
    `Description` text DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. 工作經歷表
CREATE TABLE `WorkExperiences` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Company` varchar(100) NOT NULL,
    `Position` varchar(100) NOT NULL,
    `StartDate` date DEFAULT NULL,
    `EndDate` date DEFAULT NULL,
    `IsCurrent` tinyint(1) DEFAULT 0,
    `Description` text DEFAULT NULL,
    `Achievements` text DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. 專長/技能表
CREATE TABLE `Skills` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Name` varchar(100) NOT NULL,
    `Category` varchar(50) DEFAULT NULL,
    `Level` enum('Beginner','Intermediate','Advanced','Expert') DEFAULT 'Intermediate',
    `Description` text DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. 作品集表
CREATE TABLE `Portfolios` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Description` text DEFAULT NULL,
    `TechnologyUsed` text DEFAULT NULL,
    `ProjectUrl` varchar(255) DEFAULT NULL,
    `GithubUrl` varchar(255) DEFAULT NULL,
    `ImageUrl` varchar(255) DEFAULT NULL,
    `StartDate` date DEFAULT NULL,
    `EndDate` date DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `IsFeatured` tinyint(1) DEFAULT 0,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. 行事曆表
CREATE TABLE `CalendarEvents` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Description` text DEFAULT NULL,
    `StartDate` datetime NOT NULL,
    `EndDate` datetime DEFAULT NULL,
    `IsAllDay` tinyint(1) DEFAULT 0,
    `Location` varchar(255) DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 0,
    `EventType` enum('Personal','Work','Meeting','Reminder','Other') DEFAULT 'Personal',
    `GoogleEventId` varchar(255) DEFAULT NULL,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. 工作追蹤表
CREATE TABLE `WorkTasks` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Description` text DEFAULT NULL,
    `Status` enum('Planning','InProgress','Testing','Completed','OnHold','Cancelled') DEFAULT 'Planning',
    `Priority` enum('Low','Medium','High','Urgent') DEFAULT 'Medium',
    `StartDate` datetime DEFAULT NULL,
    `DueDate` datetime DEFAULT NULL,
    `CompletedDate` datetime DEFAULT NULL,
    `EstimatedHours` decimal(5,2) DEFAULT NULL,
    `ActualHours` decimal(5,2) DEFAULT NULL,
    `Project` varchar(100) DEFAULT NULL,
    `Tags` text DEFAULT NULL,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 9. 個人待辦事項表
CREATE TABLE `TodoItems` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Description` text DEFAULT NULL,
    `IsCompleted` tinyint(1) DEFAULT 0,
    `Priority` enum('Low','Medium','High') DEFAULT 'Medium',
    `DueDate` datetime DEFAULT NULL,
    `CompletedDate` datetime DEFAULT NULL,
    `Category` varchar(50) DEFAULT NULL,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 10. 文章/網誌表
CREATE TABLE `BlogPosts` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Content` longtext NOT NULL,
    `Summary` text DEFAULT NULL,
    `IsPublished` tinyint(1) DEFAULT 0,
    `IsPublic` tinyint(1) DEFAULT 0,
    `PublishedDate` datetime DEFAULT NULL,
    `FeaturedImageUrl` varchar(255) DEFAULT NULL,
    `Tags` text DEFAULT NULL,
    `Category` varchar(50) DEFAULT NULL,
    `ViewCount` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 11. 留言板表
CREATE TABLE `GuestBookEntries` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) NOT NULL,
    `Email` varchar(100) DEFAULT NULL,
    `Website` varchar(255) DEFAULT NULL,
    `Message` text NOT NULL,
    `IsApproved` tinyint(1) DEFAULT 0,
    `IsPublic` tinyint(1) DEFAULT 1,
    `IpAddress` varchar(45) DEFAULT NULL,
    `UserAgent` varchar(500) DEFAULT NULL,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 12. 聯絡方式表
CREATE TABLE `ContactMethods` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `UserId` int(11) NOT NULL,
    `Type` enum('Email','Phone','LinkedIn','GitHub','Facebook','Twitter','Instagram','Other') NOT NULL,
    `Value` varchar(255) NOT NULL,
    `Label` varchar(100) DEFAULT NULL,
    `IsPublic` tinyint(1) DEFAULT 1,
    `IsPreferred` tinyint(1) DEFAULT 0,
    `SortOrder` int(11) DEFAULT 0,
    `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;