SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @ImageMap TABLE
(
    OldUrl nvarchar(360) NOT NULL,
    NewUrl nvarchar(360) NOT NULL
);

INSERT INTO @ImageMap (OldUrl, NewUrl)
VALUES
(N'/uploads/media/imported/content/article-ai-government-public-sector.png', N'/uploads/media/imported/content/article-ai-government-public-sector.webp'),
(N'/uploads/media/imported/content/article-enterprise-ai-admin-panel.png', N'/uploads/media/imported/content/article-enterprise-ai-admin-panel.webp'),
(N'/uploads/media/imported/content/article-enterprise-ai-assistant-managers.png', N'/uploads/media/imported/content/article-enterprise-ai-assistant-managers.webp'),
(N'/uploads/media/imported/content/article-enterprise-ai-pilot-production.png', N'/uploads/media/imported/content/article-enterprise-ai-pilot-production.webp'),
(N'/uploads/media/imported/content/article-ollama-chatgpt-azure-comparison.png', N'/uploads/media/imported/content/article-ollama-chatgpt-azure-comparison.webp'),
(N'/uploads/media/imported/content/article-ollama-enterprise-ai.png', N'/uploads/media/imported/content/article-ollama-enterprise-ai.webp'),
(N'/uploads/media/imported/content/chatbot-builder-agentic-architecture-hero-v2.png', N'/uploads/media/imported/content/chatbot-builder-agentic-architecture-hero-v2.webp');

UPDATE article
SET ImagePath = imageMap.NewUrl
FROM BlogArticles AS article
JOIN @ImageMap AS imageMap ON article.ImagePath = imageMap.OldUrl;

UPDATE product
SET HeroImagePath = imageMap.NewUrl
FROM Products AS product
JOIN @ImageMap AS imageMap ON product.HeroImagePath = imageMap.OldUrl;

UPDATE product
SET LogoPath = imageMap.NewUrl
FROM Products AS product
JOIN @ImageMap AS imageMap ON product.LogoPath = imageMap.OldUrl;

DECLARE @OldUrl nvarchar(360);
DECLARE @NewUrl nvarchar(360);

DECLARE image_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT OldUrl, NewUrl FROM @ImageMap;

OPEN image_cursor;
FETCH NEXT FROM image_cursor INTO @OldUrl, @NewUrl;

WHILE @@FETCH_STATUS = 0
BEGIN
    UPDATE BlogArticles
    SET Body = REPLACE(Body, @OldUrl, @NewUrl)
    WHERE Body LIKE N'%' + @OldUrl + N'%';

    UPDATE StaticPages
    SET Body = REPLACE(Body, @OldUrl, @NewUrl),
        Summary = REPLACE(Summary, @OldUrl, @NewUrl)
    WHERE Body LIKE N'%' + @OldUrl + N'%'
       OR Summary LIKE N'%' + @OldUrl + N'%';

    FETCH NEXT FROM image_cursor INTO @OldUrl, @NewUrl;
END

CLOSE image_cursor;
DEALLOCATE image_cursor;

UPDATE media
SET Url = imageMap.NewUrl,
    Name = RIGHT(imageMap.NewUrl, CHARINDEX(N'/', REVERSE(imageMap.NewUrl) + N'/') - 1),
    UpdatedAt = SYSDATETIMEOFFSET()
FROM MediaAssets AS media
JOIN @ImageMap AS imageMap ON media.Url = imageMap.OldUrl
WHERE NOT EXISTS (
    SELECT 1
    FROM MediaAssets AS existing
    WHERE existing.Url = imageMap.NewUrl
);

COMMIT TRANSACTION;
