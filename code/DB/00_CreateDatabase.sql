-- Personal Manager Database Creation Script
-- Created: 2025-08-12
-- Description: 建立 Personal Manager 系統資料庫

-- 建立資料庫
CREATE DATABASE IF NOT EXISTS `PersonalManager` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `PersonalManager`;

-- 設定時區
SET time_zone = '+08:00';