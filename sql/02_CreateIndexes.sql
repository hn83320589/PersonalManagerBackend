-- Personal Manager Indexes Creation Script
-- Created: 2025-08-12
-- Description: 建立索引以提升查詢效能

USE `PersonalManager`;

-- Users 表索引
CREATE INDEX `idx_users_username` ON `Users`(`Username`);
CREATE INDEX `idx_users_email` ON `Users`(`Email`);
CREATE INDEX `idx_users_active` ON `Users`(`IsActive`);

-- PersonalProfiles 表索引
CREATE INDEX `idx_personalprofiles_userid` ON `PersonalProfiles`(`UserId`);
CREATE INDEX `idx_personalprofiles_public` ON `PersonalProfiles`(`IsPublic`);

-- Educations 表索引
CREATE INDEX `idx_educations_userid` ON `Educations`(`UserId`);
CREATE INDEX `idx_educations_public_sort` ON `Educations`(`IsPublic`, `SortOrder`);
CREATE INDEX `idx_educations_dates` ON `Educations`(`StartDate`, `EndDate`);

-- WorkExperiences 表索引
CREATE INDEX `idx_workexperiences_userid` ON `WorkExperiences`(`UserId`);
CREATE INDEX `idx_workexperiences_public_sort` ON `WorkExperiences`(`IsPublic`, `SortOrder`);
CREATE INDEX `idx_workexperiences_current` ON `WorkExperiences`(`IsCurrent`);
CREATE INDEX `idx_workexperiences_dates` ON `WorkExperiences`(`StartDate`, `EndDate`);

-- Skills 表索引
CREATE INDEX `idx_skills_userid` ON `Skills`(`UserId`);
CREATE INDEX `idx_skills_category` ON `Skills`(`Category`);
CREATE INDEX `idx_skills_public_sort` ON `Skills`(`IsPublic`, `SortOrder`);

-- Portfolios 表索引
CREATE INDEX `idx_portfolios_userid` ON `Portfolios`(`UserId`);
CREATE INDEX `idx_portfolios_public_featured` ON `Portfolios`(`IsPublic`, `IsFeatured`);
CREATE INDEX `idx_portfolios_sort` ON `Portfolios`(`SortOrder`);
CREATE INDEX `idx_portfolios_dates` ON `Portfolios`(`StartDate`, `EndDate`);

-- CalendarEvents 表索引
CREATE INDEX `idx_calendarevents_userid` ON `CalendarEvents`(`UserId`);
CREATE INDEX `idx_calendarevents_dates` ON `CalendarEvents`(`StartDate`, `EndDate`);
CREATE INDEX `idx_calendarevents_public` ON `CalendarEvents`(`IsPublic`);
CREATE INDEX `idx_calendarevents_type` ON `CalendarEvents`(`EventType`);
CREATE INDEX `idx_calendarevents_google` ON `CalendarEvents`(`GoogleEventId`);

-- WorkTasks 表索引
CREATE INDEX `idx_worktasks_userid` ON `WorkTasks`(`UserId`);
CREATE INDEX `idx_worktasks_status` ON `WorkTasks`(`Status`);
CREATE INDEX `idx_worktasks_priority` ON `WorkTasks`(`Priority`);
CREATE INDEX `idx_worktasks_dates` ON `WorkTasks`(`StartDate`, `DueDate`);
CREATE INDEX `idx_worktasks_project` ON `WorkTasks`(`Project`);

-- TodoItems 表索引
CREATE INDEX `idx_todoitems_userid` ON `TodoItems`(`UserId`);
CREATE INDEX `idx_todoitems_completed` ON `TodoItems`(`IsCompleted`);
CREATE INDEX `idx_todoitems_priority` ON `TodoItems`(`Priority`);
CREATE INDEX `idx_todoitems_category` ON `TodoItems`(`Category`);
CREATE INDEX `idx_todoitems_due` ON `TodoItems`(`DueDate`);
CREATE INDEX `idx_todoitems_sort` ON `TodoItems`(`SortOrder`);

-- BlogPosts 表索引
CREATE INDEX `idx_blogposts_userid` ON `BlogPosts`(`UserId`);
CREATE INDEX `idx_blogposts_published` ON `BlogPosts`(`IsPublished`, `IsPublic`);
CREATE INDEX `idx_blogposts_published_date` ON `BlogPosts`(`PublishedDate`);
CREATE INDEX `idx_blogposts_category` ON `BlogPosts`(`Category`);
CREATE INDEX `idx_blogposts_view_count` ON `BlogPosts`(`ViewCount`);

-- GuestBookEntries 表索引
CREATE INDEX `idx_guestbook_approved_public` ON `GuestBookEntries`(`IsApproved`, `IsPublic`);
CREATE INDEX `idx_guestbook_created` ON `GuestBookEntries`(`CreatedAt`);
CREATE INDEX `idx_guestbook_email` ON `GuestBookEntries`(`Email`);

-- ContactMethods 表索引
CREATE INDEX `idx_contactmethods_userid` ON `ContactMethods`(`UserId`);
CREATE INDEX `idx_contactmethods_type` ON `ContactMethods`(`Type`);
CREATE INDEX `idx_contactmethods_public_sort` ON `ContactMethods`(`IsPublic`, `SortOrder`);
CREATE INDEX `idx_contactmethods_preferred` ON `ContactMethods`(`IsPreferred`);