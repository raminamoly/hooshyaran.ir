SET NOCOUNT ON;

DECLARE @AboutTitle nvarchar(220) = N'شرکت داده‌پردازان کوشای پاسارگاد';
DECLARE @AboutSummary nvarchar(700) = N'شرکت داده‌پردازان کوشای پاسارگاد از سال ۱۳۸۸ در طراحی و توسعه سامانه‌های نرم‌افزاری سازمانی و مکانیزاسیون فرایندها فعالیت می‌کند.';
DECLARE @AboutBody nvarchar(max) = N'
<p>شرکت داده‌پردازان کوشای پاسارگاد در سال ۱۳۸۸ متولد شد. از آن زمان تا امروز، هدف شرکت طراحی و برنامه‌نویسی سامانه‌های نرم‌افزاری باکیفیت برای حل مسائل واقعی سازمان‌ها بوده است.</p>
<p>در سال ۱۳۹۵ شرکت توانست از محصولات خود به عنوان محصولی دانش‌بنیان دفاع کند و فعالیت خود را در مسیر تحقق مکانیزاسیون فرایندهای سازمانی گسترش دهد. صداقت، کیفیت و وجدان کاری از ابتدا تا امروز در ارائه خدمات و محصولات، سرلوحه فعالیت شرکت بوده است.</p>
<p>مدیرعامل و رئیس هیئت‌مدیره شرکت، مهندس رامین چلای آملی است.</p>
<ul>
<li>نیازسنجی، تحلیل و پیاده‌سازی نرم‌افزارهای کاربردی و سازمانی</li>
<li>مکانیزاسیون فرایندهای سازمانی در بستر وب</li>
<li>یکپارچه‌سازی اطلاعات جزیره‌ای سازمان</li>
<li>طراحی انباره داده متمرکز اطلاعاتی</li>
<li>طراحی و پیاده‌سازی داشبوردهای مدیریتی و استراتژیک سازمان</li>
<li>مانیتورینگ شاخص‌های کلیدی عملکردی</li>
</ul>';
DECLARE @SeoTitle nvarchar(220) = N'درباره ما | شرکت داده‌پردازان کوشای پاسارگاد';
DECLARE @SeoDescription nvarchar(320) = N'معرفی شرکت داده‌پردازان کوشای پاسارگاد، سوابق دانش‌بنیان، توانمندی‌ها و مجوزهای شرکت در توسعه سامانه‌های نرم‌افزاری سازمانی.';
DECLARE @SeoKeywords nvarchar(500) = N'درباره شرکت داده‌پردازان کوشای پاسارگاد, شرکت دانش بنیان نرم افزار, مکانیزاسیون فرایندهای سازمانی, سامانه های نرم افزاری سازمانی, مجوزهای شرکت';

IF EXISTS (SELECT 1 FROM dbo.StaticPages WHERE [Key] = N'about')
BEGIN
    UPDATE dbo.StaticPages
    SET
        Title = @AboutTitle,
        Slug = N'about',
        Summary = @AboutSummary,
        Body = @AboutBody,
        IsPublished = 1,
        SeoTitle = @SeoTitle,
        SeoDescription = @SeoDescription,
        SeoKeywords = @SeoKeywords
    WHERE [Key] = N'about';
END
ELSE
BEGIN
    INSERT INTO dbo.StaticPages
        ([Key], Title, Slug, Summary, Body, IsPublished, SeoTitle, SeoDescription, SeoKeywords)
    VALUES
        (N'about', @AboutTitle, N'about', @AboutSummary, @AboutBody, 1, @SeoTitle, @SeoDescription, @SeoKeywords);
END

IF EXISTS (SELECT 1 FROM dbo.SeoMetadata WHERE PageKey = N'about')
BEGIN
    UPDATE dbo.SeoMetadata
    SET
        Title = @SeoTitle,
        Description = @SeoDescription,
        Keywords = @SeoKeywords,
        CanonicalPath = N'/about'
    WHERE PageKey = N'about';
END
ELSE
BEGIN
    INSERT INTO dbo.SeoMetadata
        (PageKey, Title, Description, Keywords, CanonicalPath)
    VALUES
        (N'about', @SeoTitle, @SeoDescription, @SeoKeywords, N'/about');
END

SELECT N'About page content patch applied.' AS [Status];
