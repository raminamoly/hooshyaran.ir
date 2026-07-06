SET NOCOUNT ON;

DECLARE @ContactTitle nvarchar(220) = N'تماس با هوش‌یاران';
DECLARE @ContactSummary nvarchar(700) = N'برای دریافت مشاوره، درخواست دمو یا بررسی سناریوی سازمانی خود با هوش‌یاران در تماس باشید.';
DECLARE @ContactBody nvarchar(max) = N'<p>هوش‌یاران راهکارهای هوش مصنوعی سازمانی را برای شرکت‌ها و سازمان‌هایی آماده می‌کند که می‌خواهند از مدل‌های زبانی، اتوماسیون هوشمند و تحلیل داده در فرایندهای واقعی استفاده کنند.</p><p>تلفن ثابت: 021-45378 - 021-88749130</p><p>ایمیل: info@jaryansoft.com</p><p>آدرس: تهران، خیابان بهشتی، خ دهم قائم مقام، پلاک 24، واحد 29</p><p>موقعیت شرکت: 35.7276757, 51.4189552</p>';
DECLARE @SeoTitle nvarchar(220) = N'تماس با هوش‌یاران | شرکت داده‌پردازان کوشای پاسارگاد';
DECLARE @SeoDescription nvarchar(320) = N'اطلاعات تماس هوش‌یاران برای درخواست دمو، جلسه معرفی و بررسی راهکارهای هوش مصنوعی سازمانی.';
DECLARE @SeoKeywords nvarchar(500) = N'تماس با هوش‌یاران, داده‌پردازان کوشای پاسارگاد, درخواست دمو هوش مصنوعی سازمانی, مشاوره AI سازمانی';

IF EXISTS (SELECT 1 FROM dbo.StaticPages WHERE [Key] = N'contact')
BEGIN
    UPDATE dbo.StaticPages
    SET
        Title = @ContactTitle,
        Slug = N'contact',
        Summary = @ContactSummary,
        Body = @ContactBody,
        IsPublished = 1,
        SeoTitle = @SeoTitle,
        SeoDescription = @SeoDescription,
        SeoKeywords = @SeoKeywords
    WHERE [Key] = N'contact';
END
ELSE
BEGIN
    INSERT INTO dbo.StaticPages
        ([Key], Title, Slug, Summary, Body, IsPublished, SeoTitle, SeoDescription, SeoKeywords)
    VALUES
        (N'contact', @ContactTitle, N'contact', @ContactSummary, @ContactBody, 1, @SeoTitle, @SeoDescription, @SeoKeywords);
END

IF EXISTS (SELECT 1 FROM dbo.SeoMetadata WHERE PageKey = N'contact')
BEGIN
    UPDATE dbo.SeoMetadata
    SET
        Title = @SeoTitle,
        Description = @SeoDescription,
        Keywords = @SeoKeywords,
        CanonicalPath = N'/contact'
    WHERE PageKey = N'contact';
END
ELSE
BEGIN
    INSERT INTO dbo.SeoMetadata
        (PageKey, Title, Description, Keywords, CanonicalPath)
    VALUES
        (N'contact', @SeoTitle, @SeoDescription, @SeoKeywords, N'/contact');
END

SELECT N'Contact page content patch applied.' AS [Status];
