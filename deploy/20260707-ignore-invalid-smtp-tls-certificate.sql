BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707202912_AddIgnoreInvalidSmtpTlsCertificate'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [IgnoreInvalidTlsCertificate] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707202912_AddIgnoreInvalidSmtpTlsCertificate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707202912_AddIgnoreInvalidSmtpTlsCertificate', N'10.0.8');
END;

COMMIT;
GO

