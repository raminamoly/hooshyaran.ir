SET NOCOUNT ON;

IF COL_LENGTH(N'[dbo].[AdminUsers]', N'MobileNumber') IS NULL
BEGIN
    ALTER TABLE [dbo].[AdminUsers]
        ADD [MobileNumber] NVARCHAR(32) NOT NULL
            CONSTRAINT [DF_AdminUsers_MobileNumber] DEFAULT N'';
    ALTER TABLE [dbo].[AdminUsers] DROP CONSTRAINT [DF_AdminUsers_MobileNumber];
END;

IF COL_LENGTH(N'[dbo].[AdminUsers]', N'ReceiveDemoRequestNotifications') IS NULL
BEGIN
    ALTER TABLE [dbo].[AdminUsers]
        ADD [ReceiveDemoRequestNotifications] BIT NOT NULL
            CONSTRAINT [DF_AdminUsers_ReceiveDemoRequestNotifications] DEFAULT 0;
    ALTER TABLE [dbo].[AdminUsers] DROP CONSTRAINT [DF_AdminUsers_ReceiveDemoRequestNotifications];
END;

UPDATE [dbo].[AdminUsers]
SET [MobileNumber] = N''
WHERE [MobileNumber] IS NULL;

UPDATE [dbo].[AdminUsers]
SET [ReceiveDemoRequestNotifications] = 0
WHERE [ReceiveDemoRequestNotifications] IS NULL;

SELECT N'Demo request notification recipient columns patch applied.' AS [Status];
