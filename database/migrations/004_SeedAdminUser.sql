USE [EnterpriseSmartHrmDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF EXISTS (
    SELECT 1
    FROM dbo.DatabaseMigrations
    WHERE MigrationId = N'004_SeedAdminUser')
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Default administrator account.
    -- Username: admin   Password: Admin@123
    -- PBKDF2-SHA512, 210000 iterations, 16-byte salt, 32-byte hash (matches Pbkdf2PasswordHasher).
    -- IMPORTANT: change this password immediately after first login.
    DECLARE @AdminUsername NVARCHAR(100) = N'admin';
    DECLARE @AdminNormalizedUsername NVARCHAR(100) = N'ADMIN';
    DECLARE @AdminEmail NVARCHAR(256) = N'admin@enterprisesmarthrm.local';
    DECLARE @AdminNormalizedEmail NVARCHAR(256) = N'ADMIN@ENTERPRISESMARTHRM.LOCAL';
    DECLARE @AdminPasswordHash NVARCHAR(512) =
        N'pbkdf2-sha512$1$210000$7qtEF3wcU8Ibfu99UskVLA==$FVHnitQDMXhJpDzBKe4BgI+5qckjjFIaGmb+/Wa0iSw=';

    IF NOT EXISTS (
        SELECT 1 FROM auth.Users
        WHERE NormalizedUsername = @AdminNormalizedUsername AND IsDeleted = 0)
    BEGIN
        INSERT INTO auth.Users
            (Username, NormalizedUsername, Email, NormalizedEmail, PasswordHash,
             IsActive, FailedLoginCount, PasswordChangedAtUtc)
        VALUES
            (@AdminUsername, @AdminNormalizedUsername, @AdminEmail, @AdminNormalizedEmail, @AdminPasswordHash,
             1, 0, SYSUTCDATETIME());
    END;

    DECLARE @AdminUserId INT =
        (SELECT Id FROM auth.Users WHERE NormalizedUsername = @AdminNormalizedUsername AND IsDeleted = 0);

    DECLARE @AdminRoleId INT =
        (SELECT Id FROM auth.Roles WHERE NormalizedName = N'ADMIN' AND IsDeleted = 0);

    IF @AdminUserId IS NOT NULL
       AND @AdminRoleId IS NOT NULL
       AND NOT EXISTS (
            SELECT 1 FROM auth.UserRoles
            WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO auth.UserRoles (UserId, RoleId)
        VALUES (@AdminUserId, @AdminRoleId);
    END;

    INSERT INTO dbo.DatabaseMigrations (MigrationId, Description)
    VALUES (N'004_SeedAdminUser', N'Seed default administrator account.');

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
