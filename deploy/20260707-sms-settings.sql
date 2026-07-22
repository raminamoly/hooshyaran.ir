SET NOCOUNT ON;

IF COL_LENGTH(N'[dbo].[SiteSettings]', N'SmsApiUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[SiteSettings]
        ADD [SmsApiUrl] NVARCHAR(260) NOT NULL
            CONSTRAINT [DF_SiteSettings_SmsApiUrl] DEFAULT N'https://api.sms.ir/v1/send/verify';
    ALTER TABLE [dbo].[SiteSettings] DROP CONSTRAINT [DF_SiteSettings_SmsApiUrl];
END;

IF COL_LENGTH(N'[dbo].[SiteSettings]', N'SmsApiKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[SiteSettings]
        ADD [SmsApiKey] NVARCHAR(500) NOT NULL
            CONSTRAINT [DF_SiteSettings_SmsApiKey] DEFAULT N'';
    ALTER TABLE [dbo].[SiteSettings] DROP CONSTRAINT [DF_SiteSettings_SmsApiKey];
END;

IF COL_LENGTH(N'[dbo].[SiteSettings]', N'SmsOtpTemplateId') IS NULL
BEGIN
    ALTER TABLE [dbo].[SiteSettings]
        ADD [SmsOtpTemplateId] INT NOT NULL
            CONSTRAINT [DF_SiteSettings_SmsOtpTemplateId] DEFAULT 160052;
    ALTER TABLE [dbo].[SiteSettings] DROP CONSTRAINT [DF_SiteSettings_SmsOtpTemplateId];
END;

IF COL_LENGTH(N'[dbo].[SiteSettings]', N'SmsMessageTemplateId') IS NULL
BEGIN
    ALTER TABLE [dbo].[SiteSettings]
        ADD [SmsMessageTemplateId] INT NOT NULL
            CONSTRAINT [DF_SiteSettings_SmsMessageTemplateId] DEFAULT 391212;
    ALTER TABLE [dbo].[SiteSettings] DROP CONSTRAINT [DF_SiteSettings_SmsMessageTemplateId];
END;

UPDATE [dbo].[SiteSettings]
SET [SmsApiUrl] = N'https://api.sms.ir/v1/send/verify'
WHERE LTRIM(RTRIM([SmsApiUrl])) = N'';

UPDATE [dbo].[SiteSettings]
SET [SmsOtpTemplateId] = 160052
WHERE [SmsOtpTemplateId] <= 0;

UPDATE [dbo].[SiteSettings]
SET [SmsMessageTemplateId] = 391212
WHERE [SmsMessageTemplateId] <= 0;

SELECT N'SMS settings patch applied.' AS [Status];
