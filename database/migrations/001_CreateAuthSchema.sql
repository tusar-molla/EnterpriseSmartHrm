USE [EnterpriseSmartHrmDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.DatabaseMigrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DatabaseMigrations
    (
        MigrationId NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        AppliedAtUtc DATETIME2(7) NOT NULL
            CONSTRAINT DF_DatabaseMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_DatabaseMigrations PRIMARY KEY (MigrationId)
    );
END;

IF EXISTS (
    SELECT 1
    FROM dbo.DatabaseMigrations
    WHERE MigrationId = N'001_CreateAuthSchema')
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'auth') IS NULL
    BEGIN
        EXEC(N'CREATE SCHEMA auth AUTHORIZATION dbo;');
    END;

    CREATE TABLE auth.Users
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        NormalizedUsername NVARCHAR(100) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        NormalizedEmail NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        EmployeeId INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        LastLoginAtUtc DATETIME2(7) NULL,
        FailedLoginCount INT NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT (0),
        LockoutEndAtUtc DATETIME2(7) NULL,
        PasswordChangedAtUtc DATETIME2(7) NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy INT NULL,
        UpdatedAtUtc DATETIME2(7) NULL,
        UpdatedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0),
        DeletedAtUtc DATETIME2(7) NULL,
        DeletedBy INT NULL,
        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT CK_Users_FailedLoginCount CHECK (FailedLoginCount >= 0)
    );

    CREATE UNIQUE INDEX UX_Users_NormalizedUsername
        ON auth.Users (NormalizedUsername)
        WHERE IsDeleted = 0;

    CREATE UNIQUE INDEX UX_Users_NormalizedEmail
        ON auth.Users (NormalizedEmail)
        WHERE IsDeleted = 0;

    CREATE INDEX IX_Users_EmployeeId
        ON auth.Users (EmployeeId)
        WHERE EmployeeId IS NOT NULL AND IsDeleted = 0;

    CREATE TABLE auth.Roles
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        NormalizedName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsSystemRole BIT NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Roles_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy INT NULL,
        UpdatedAtUtc DATETIME2(7) NULL,
        UpdatedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT (0),
        DeletedAtUtc DATETIME2(7) NULL,
        DeletedBy INT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX UX_Roles_NormalizedName
        ON auth.Roles (NormalizedName)
        WHERE IsDeleted = 0;

    CREATE TABLE auth.Permissions
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        ModuleName NVARCHAR(100) NOT NULL,
        PermissionKey NVARCHAR(150) NOT NULL,
        DisplayName NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Permissions_IsActive DEFAULT (1),
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Permissions_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2(7) NULL,
        CONSTRAINT PK_Permissions PRIMARY KEY (Id),
        CONSTRAINT UQ_Permissions_PermissionKey UNIQUE (PermissionKey)
    );

    CREATE INDEX IX_Permissions_ModuleName
        ON auth.Permissions (ModuleName, IsActive);

    CREATE TABLE auth.UserRoles
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_UserRoles_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy INT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (Id),
        CONSTRAINT UQ_UserRoles_UserId_RoleId UNIQUE (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users_UserId
            FOREIGN KEY (UserId) REFERENCES auth.Users (Id),
        CONSTRAINT FK_UserRoles_Roles_RoleId
            FOREIGN KEY (RoleId) REFERENCES auth.Roles (Id)
    );

    CREATE INDEX IX_UserRoles_RoleId
        ON auth.UserRoles (RoleId);

    CREATE TABLE auth.RolePermissions
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        IsAllowed BIT NOT NULL CONSTRAINT DF_RolePermissions_IsAllowed DEFAULT (1),
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_RolePermissions_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy INT NULL,
        UpdatedAtUtc DATETIME2(7) NULL,
        UpdatedBy INT NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY (Id),
        CONSTRAINT UQ_RolePermissions_RoleId_PermissionId UNIQUE (RoleId, PermissionId),
        CONSTRAINT FK_RolePermissions_Roles_RoleId
            FOREIGN KEY (RoleId) REFERENCES auth.Roles (Id),
        CONSTRAINT FK_RolePermissions_Permissions_PermissionId
            FOREIGN KEY (PermissionId) REFERENCES auth.Permissions (Id)
    );

    CREATE INDEX IX_RolePermissions_PermissionId
        ON auth.RolePermissions (PermissionId);

    CREATE TABLE auth.RefreshTokens
    (
        Id BIGINT IDENTITY(1, 1) NOT NULL,
        UserId INT NOT NULL,
        TokenHash CHAR(64) NOT NULL,
        ExpiresAtUtc DATETIME2(7) NOT NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedByIp NVARCHAR(45) NULL,
        RevokedAtUtc DATETIME2(7) NULL,
        RevokedByIp NVARCHAR(45) NULL,
        ReplacedByTokenHash CHAR(64) NULL,
        RevokeReason NVARCHAR(500) NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
        CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash),
        CONSTRAINT FK_RefreshTokens_Users_UserId
            FOREIGN KEY (UserId) REFERENCES auth.Users (Id)
    );

    CREATE INDEX IX_RefreshTokens_UserId_ExpiresAtUtc
        ON auth.RefreshTokens (UserId, ExpiresAtUtc);

    CREATE TABLE auth.LoginHistories
    (
        Id BIGINT IDENTITY(1, 1) NOT NULL,
        UserId INT NULL,
        UsernameOrEmail NVARCHAR(256) NOT NULL,
        IsSuccessful BIT NOT NULL,
        FailureReason NVARCHAR(500) NULL,
        IpAddress NVARCHAR(45) NULL,
        UserAgent NVARCHAR(1000) NULL,
        OccurredAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_LoginHistories_OccurredAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_LoginHistories PRIMARY KEY (Id),
        CONSTRAINT FK_LoginHistories_Users_UserId
            FOREIGN KEY (UserId) REFERENCES auth.Users (Id)
    );

    CREATE INDEX IX_LoginHistories_UserId_OccurredAtUtc
        ON auth.LoginHistories (UserId, OccurredAtUtc DESC);

    CREATE INDEX IX_LoginHistories_UsernameOrEmail_OccurredAtUtc
        ON auth.LoginHistories (UsernameOrEmail, OccurredAtUtc DESC);

    INSERT INTO dbo.DatabaseMigrations (MigrationId, Description)
    VALUES (N'001_CreateAuthSchema', N'Create authentication and authorization schema.');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO
