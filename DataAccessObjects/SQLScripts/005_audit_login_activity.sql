USE [HotelManagementSystem];
GO

IF OBJECT_ID(N'dbo.user_login_sessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.user_login_sessions
    (
        login_session_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        user_id INT NOT NULL,
        login_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_login_sessions_login_at DEFAULT SYSUTCDATETIME(),
        logout_at_utc DATETIME2 NULL,
        last_seen_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_login_sessions_last_seen DEFAULT SYSUTCDATETIME(),
        machine_name NVARCHAR(100) NOT NULL CONSTRAINT DF_user_login_sessions_machine DEFAULT (N'Unknown'),
        windows_user NVARCHAR(100) NOT NULL CONSTRAINT DF_user_login_sessions_windows_user DEFAULT (N'Unknown'),
        ip_address NVARCHAR(45) NOT NULL CONSTRAINT DF_user_login_sessions_ip DEFAULT (N'Unknown'),
        os_version NVARCHAR(200) NOT NULL CONSTRAINT DF_user_login_sessions_os DEFAULT (N'Unknown'),
        app_version NVARCHAR(50) NOT NULL CONSTRAINT DF_user_login_sessions_app DEFAULT (N'Unknown'),
        device_type NVARCHAR(50) NOT NULL CONSTRAINT DF_user_login_sessions_device DEFAULT (N'Windows Desktop'),
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_user_login_sessions_status DEFAULT (N'Active')
    );
END
GO

IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.user_activity_logs
    (
        activity_log_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        login_session_id INT NULL,
        actor_user_id INT NULL,
        target_user_id INT NULL,
        attempted_username NVARCHAR(50) NULL,
        action_type NVARCHAR(50) NOT NULL,
        entity_name NVARCHAR(100) NOT NULL,
        entity_id NVARCHAR(100) NULL,
        description NVARCHAR(1000) NOT NULL,
        old_values_json NVARCHAR(MAX) NULL,
        new_values_json NVARCHAR(MAX) NULL,
        result NVARCHAR(30) NOT NULL CONSTRAINT DF_user_activity_logs_result DEFAULT (N'Success'),
        error_message NVARCHAR(1000) NULL,
        occurred_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_activity_logs_occurred DEFAULT SYSUTCDATETIME(),
        machine_name NVARCHAR(100) NOT NULL CONSTRAINT DF_user_activity_logs_machine DEFAULT (N'Unknown'),
        ip_address NVARCHAR(45) NOT NULL CONSTRAINT DF_user_activity_logs_ip DEFAULT (N'Unknown'),
        device_type NVARCHAR(50) NOT NULL CONSTRAINT DF_user_activity_logs_device DEFAULT (N'Windows Desktop')
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_login_sessions_users')
BEGIN
    ALTER TABLE dbo.user_login_sessions
        ADD CONSTRAINT FK_user_login_sessions_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_login_sessions')
BEGIN
    ALTER TABLE dbo.user_activity_logs
        ADD CONSTRAINT FK_user_activity_logs_login_sessions
        FOREIGN KEY (login_session_id) REFERENCES dbo.user_login_sessions(login_session_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_actor_users')
BEGIN
    ALTER TABLE dbo.user_activity_logs
        ADD CONSTRAINT FK_user_activity_logs_actor_users
        FOREIGN KEY (actor_user_id) REFERENCES dbo.users(user_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_target_users')
BEGIN
    ALTER TABLE dbo.user_activity_logs
        ADD CONSTRAINT FK_user_activity_logs_target_users
        FOREIGN KEY (target_user_id) REFERENCES dbo.users(user_id);
END
GO

IF OBJECT_ID(N'dbo.CK_user_login_sessions_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.user_login_sessions
        ADD CONSTRAINT CK_user_login_sessions_status CHECK (status IN (N'Active', N'LoggedOut'));
END
GO

IF OBJECT_ID(N'dbo.CK_user_activity_logs_result', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.user_activity_logs
        ADD CONSTRAINT CK_user_activity_logs_result CHECK (result IN (N'Success', N'Failed'));
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_login_sessions_user_time' AND object_id = OBJECT_ID(N'dbo.user_login_sessions'))
BEGIN
    CREATE INDEX IX_user_login_sessions_user_time ON dbo.user_login_sessions(user_id, login_at_utc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
BEGIN
    CREATE INDEX IX_user_activity_logs_time ON dbo.user_activity_logs(occurred_at_utc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_actor_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
BEGIN
    CREATE INDEX IX_user_activity_logs_actor_time ON dbo.user_activity_logs(actor_user_id, occurred_at_utc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_target_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
BEGIN
    CREATE INDEX IX_user_activity_logs_target_time ON dbo.user_activity_logs(target_user_id, occurred_at_utc DESC);
END
GO
