USE [master];
GO

IF DB_ID(N'HotelManagementSystem') IS NULL
BEGIN
    PRINT N'Creating database [HotelManagementSystem]...';
    CREATE DATABASE [HotelManagementSystem];
END
ELSE
BEGIN
    PRINT N'Database [HotelManagementSystem] already exists. Skipping creation.';
END
GO
