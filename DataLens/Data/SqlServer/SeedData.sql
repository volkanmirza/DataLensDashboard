-- DataLens Seed Data for SQL Server
USE DataLensDb;
GO

-- Insert Admin User (Password: Admin123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedDate, IsActive, FirstName, LastName, 
                      Department, Position, PhoneNumber, Biography, Language, Theme, TimeZone,
                      EmailNotifications, BrowserNotifications, DashboardShared, SecurityAlerts,
                      ProfileVisibility, AllowDashboardSharing, TrackActivity, TwoFactorEnabled)
    VALUES (
        NEWID(),
        'admin',
        'admin@datalens.com',
        '$2a$11$8K1p/a0dL2LkqvQOuiOX2uy7YhFRZpfVpejp4JIBjOxddHKHf9H1W', -- BCrypt hash for 'Admin123!'
        'Admin',
        GETUTCDATE(),
        1,
        'System',
        'Administrator',
        'IT',
        'System Administrator',
        '+90 555 123 4567',
        'DataLens sistem yöneticisi. Tüm sistem operasyonlarından sorumludur.',
        'tr',
        'light',
        'Turkey Standard Time',
        1,
        1,
        1,
        1,
        'Public',
        1,
        1,
        0
    );
END
GO

-- Insert Designer User (Password: Designer123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'designer')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedDate, IsActive, FirstName, LastName,
                      Department, Position, PhoneNumber, Biography, Language, Theme, TimeZone,
                      EmailNotifications, BrowserNotifications, DashboardShared, SecurityAlerts,
                      ProfileVisibility, AllowDashboardSharing, TrackActivity, TwoFactorEnabled)
    VALUES (
        NEWID(),
        'designer',
        'designer@datalens.com',
        '$2a$11$9L2q/b1eM3MlrvRPvjPY3vz8ZiFSaqgWqfkq5KJCkPyeeiLIg0I2X', -- BCrypt hash for 'Designer123!'
        'Designer',
        GETUTCDATE(),
        1,
        'John',
        'Designer',
        'Analytics',
        'Senior Dashboard Designer',
        '+90 555 234 5678',
        'Deneyimli dashboard tasarımcısı. Veri görselleştirme ve analitik dashboard geliştirme konularında uzman.',
        'tr',
        'dark',
        'Turkey Standard Time',
        1,
        0,
        1,
        1,
        'Private',
        1,
        1,
        1
    );
END
GO

-- Insert Viewer User (Password: Viewer123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'viewer')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedDate, IsActive, FirstName, LastName,
                      Department, Position, PhoneNumber, Biography, Language, Theme, TimeZone,
                      EmailNotifications, BrowserNotifications, DashboardShared, SecurityAlerts,
                      ProfileVisibility, AllowDashboardSharing, TrackActivity, TwoFactorEnabled)
    VALUES (
        NEWID(),
        'viewer',
        'viewer@datalens.com',
        '$2a$11$0M3r/c2fN4NmswSQwkQZ4w09AjGTbrhXrgll6LKDlQzffjMJh1J3Y', -- BCrypt hash for 'Viewer123!'
        'Viewer',
        GETUTCDATE(),
        1,
        'Jane',
        'Viewer',
        'Sales',
        'Sales Analyst',
        '+90 555 345 6789',
        'Satış departmanında çalışan analitik uzmanı. Raporları inceleyerek satış performansını takip eder.',
        'en',
        'light',
        'Turkey Standard Time',
        0,
        1,
        0,
        0,
        'Friends',
        0,
        0,
        0
    );
END
GO

-- Insert Sample User Groups
DECLARE @AdminUserId NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'admin');

IF NOT EXISTS (SELECT 1 FROM UserGroups WHERE GroupName = 'Administrators')
BEGIN
    INSERT INTO UserGroups (Id, GroupName, Description, CreatedDate, IsActive, CreatedBy)
    VALUES (
        NEWID(),
        'Administrators',
        'System administrators with full access',
        GETUTCDATE(),
        1,
        @AdminUserId
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM UserGroups WHERE GroupName = 'Dashboard Designers')
BEGIN
    DECLARE @AdminUserId2 NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'admin');
    INSERT INTO UserGroups (Id, GroupName, Description, CreatedDate, IsActive, CreatedBy)
    VALUES (
        NEWID(),
        'Dashboard Designers',
        'Users who can create and edit dashboards',
        GETUTCDATE(),
        1,
        @AdminUserId2
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM UserGroups WHERE GroupName = 'Report Viewers')
BEGIN
    DECLARE @AdminUserId3 NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'admin');
    INSERT INTO UserGroups (Id, GroupName, Description, CreatedDate, IsActive, CreatedBy)
    VALUES (
        NEWID(),
        'Report Viewers',
        'Users who can only view dashboards',
        GETUTCDATE(),
        1,
        @AdminUserId3
    );
END
GO

-- Add users to groups
DECLARE @AdminId NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'admin');
DECLARE @DesignerId NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'designer');
DECLARE @ViewerId NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'viewer');
DECLARE @AdminGroupId NVARCHAR(50) = (SELECT Id FROM UserGroups WHERE GroupName = 'Administrators');
DECLARE @DesignerGroupId NVARCHAR(50) = (SELECT Id FROM UserGroups WHERE GroupName = 'Dashboard Designers');
DECLARE @ViewerGroupId NVARCHAR(50) = (SELECT Id FROM UserGroups WHERE GroupName = 'Report Viewers');

-- Add admin to administrators group
IF NOT EXISTS (SELECT 1 FROM UserGroupMembers WHERE UserId = @AdminId AND GroupId = @AdminGroupId)
BEGIN
    INSERT INTO UserGroupMembers (Id, UserId, GroupId, JoinedDate, AddedBy)
    VALUES (NEWID(), @AdminId, @AdminGroupId, GETUTCDATE(), @AdminId);
END

-- Add designer to designers group
IF NOT EXISTS (SELECT 1 FROM UserGroupMembers WHERE UserId = @DesignerId AND GroupId = @DesignerGroupId)
BEGIN
    INSERT INTO UserGroupMembers (Id, UserId, GroupId, JoinedDate, AddedBy)
    VALUES (NEWID(), @DesignerId, @DesignerGroupId, GETUTCDATE(), @AdminId);
END

-- Add viewer to viewers group
IF NOT EXISTS (SELECT 1 FROM UserGroupMembers WHERE UserId = @ViewerId AND GroupId = @ViewerGroupId)
BEGIN
    INSERT INTO UserGroupMembers (Id, UserId, GroupId, JoinedDate, AddedBy)
    VALUES (NEWID(), @ViewerId, @ViewerGroupId, GETUTCDATE(), @AdminId);
END
GO

-- Insert Sample Dashboard
DECLARE @AdminUserId4 NVARCHAR(50) = (SELECT Id FROM Users WHERE Username = 'admin');

IF NOT EXISTS (SELECT 1 FROM Dashboards WHERE Name = 'Sample Sales Dashboard')
BEGIN
    INSERT INTO Dashboards (Id, Name, Description, DashboardData, CreatedBy, CreatedDate, IsActive, IsPublic, Category)
    VALUES (
        NEWID(),
        'Sample Sales Dashboard',
        'A sample dashboard showing sales metrics',
        '{"version":"1.0","items":[{"type":"chart","title":"Sales Overview"}]}',
        @AdminUserId4,
        GETUTCDATE(),
        1,
        1,
        'Sales'
    );
END
GO

PRINT 'Seed data inserted successfully!';
GO