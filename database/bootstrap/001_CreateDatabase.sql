USE [master];
GO

IF DB_ID(N'EnterpriseSmartHrmDb') IS NULL
BEGIN
    CREATE DATABASE [EnterpriseSmartHrmDb];
END;
GO
