-- DataLens Database Schema for SQL Server
-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DataLensDb')
BEGIN
    CREATE DATABASE DataLensDb;
END
GO

USE DataLensDb;
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id NVARCHAR(50) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'Viewer',
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        LastLoginDate DATETIME2 NULL
    );
    
    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_Role ON Users(Role);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);
END
GO

-- UserGroups Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserGroups' AND xtype='U')
BEGIN
    CREATE TABLE UserGroups (
        Id NVARCHAR(50) PRIMARY KEY,
        GroupName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy NVARCHAR(50) NOT NULL,
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_UserGroups_GroupName ON UserGroups(GroupName);
    CREATE INDEX IX_UserGroups_IsActive ON UserGroups(IsActive);
    CREATE INDEX IX_UserGroups_CreatedBy ON UserGroups(CreatedBy);
END
GO

-- UserGroupMembers Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserGroupMembers' AND xtype='U')
BEGIN
    CREATE TABLE UserGroupMembers (
        Id NVARCHAR(50) PRIMARY KEY,
        UserId NVARCHAR(50) NOT NULL,
        GroupId NVARCHAR(50) NOT NULL,
        JoinedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AddedBy NVARCHAR(50) NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (GroupId) REFERENCES UserGroups(Id) ON DELETE CASCADE,
        FOREIGN KEY (AddedBy) REFERENCES Users(Id),
        UNIQUE(UserId, GroupId)
    );
    
    CREATE INDEX IX_UserGroupMembers_UserId ON UserGroupMembers(UserId);
    CREATE INDEX IX_UserGroupMembers_GroupId ON UserGroupMembers(GroupId);
END
GO

-- Dashboards Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Dashboards' AND xtype='U')
BEGIN
    CREATE TABLE Dashboards (
        Id NVARCHAR(50) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        DashboardData NVARCHAR(MAX) NOT NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastModifiedDate DATETIME2 NULL,
        LastModifiedBy NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsPublic BIT NOT NULL DEFAULT 0,
        Category NVARCHAR(50) NOT NULL DEFAULT 'General',
        Tags NVARCHAR(500) NULL, -- JSON array of tags
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        FOREIGN KEY (LastModifiedBy) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_Dashboards_Name ON Dashboards(Name);
    CREATE INDEX IX_Dashboards_CreatedBy ON Dashboards(CreatedBy);
    CREATE INDEX IX_Dashboards_IsActive ON Dashboards(IsActive);
    CREATE INDEX IX_Dashboards_IsPublic ON Dashboards(IsPublic);
    CREATE INDEX IX_Dashboards_Category ON Dashboards(Category);
END
GO

-- DashboardPermissions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DashboardPermissions' AND xtype='U')
BEGIN
    CREATE TABLE DashboardPermissions (
        Id NVARCHAR(50) PRIMARY KEY,
        DashboardId NVARCHAR(50) NOT NULL,
        UserId NVARCHAR(50) NULL,
        GroupId NVARCHAR(50) NULL,
        PermissionType NVARCHAR(20) NOT NULL,
        GrantedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        GrantedBy NVARCHAR(50) NOT NULL,
        ExpiryDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        FOREIGN KEY (DashboardId) REFERENCES Dashboards(Id) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (GroupId) REFERENCES UserGroups(Id),
        FOREIGN KEY (GrantedBy) REFERENCES Users(Id),
        CHECK ((UserId IS NOT NULL AND GroupId IS NULL) OR (UserId IS NULL AND GroupId IS NOT NULL))
    );
    
    CREATE INDEX IX_DashboardPermissions_DashboardId ON DashboardPermissions(DashboardId);
    CREATE INDEX IX_DashboardPermissions_UserId ON DashboardPermissions(UserId);
    CREATE INDEX IX_DashboardPermissions_GroupId ON DashboardPermissions(GroupId);
    CREATE INDEX IX_DashboardPermissions_PermissionType ON DashboardPermissions(PermissionType);
    CREATE INDEX IX_DashboardPermissions_IsActive ON DashboardPermissions(IsActive);
END
GO

PRINT 'DataLens database schema created successfully!';
GO