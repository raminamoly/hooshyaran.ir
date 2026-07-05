using Hooshyaran.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public static class ProductSeedData
{
    private const string ChatbotBuilderSlug = "chatbot-builder";
    private const string ChatbotBuilderPageKey = "product-chatbot-builder";
    private const string ChatbotBuilderHeroUrl = "/uploads/media/imported/content/chatbot-builder-agentic-architecture-hero-v2.png";
    private const string ChatbotBuilderArchitectureUrl = "/uploads/media/imported/content/chatbot-builder-sales-agent-architecture.jpg";

    public static async Task SeedAsync(HooshyaranDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var category = await EnsureCategoryAsync(dbContext);
        await EnsureMediaAssetAsync(dbContext, now);

        var tags = new[]
        {
            await EnsureTagAsync(dbContext, "چتبات سازمانی", "enterprise-chatbot", "ساخت و مدیریت چتبات‌های اختصاصی برای سازمان‌ها و شرکت‌ها", now),
            await EnsureTagAsync(dbContext, "RAG", "rag", "پاسخ‌گویی بر اساس اسناد، فایل‌ها و دانش داخلی سازمان", now),
            await EnsureTagAsync(dbContext, "ایجنت هوش مصنوعی", "ai-agent", "طراحی ایجنت‌های تخصصی برای اجرای وظایف هوشمند", now),
            await EnsureTagAsync(dbContext, "AI خصوصی", "private-ai", "استقرار و کنترل هوش مصنوعی در مرز داده و سیاست‌های سازمان", now)
        };

        await dbContext.SaveChangesAsync();

        var product = await dbContext.Products
            .Include(item => item.ProductTags)
            .SingleOrDefaultAsync(item => item.Slug == ChatbotBuilderSlug);

        var isNewProduct = product is null;
        if (product is null)
        {
            product = new Product { Slug = ChatbotBuilderSlug };
            dbContext.Products.Add(product);
            ApplyChatbotBuilderDefaults(product, category.Id);
        }

        await dbContext.SaveChangesAsync();

        if (isNewProduct || !product.ProductTags.Any())
        {
            SyncProductTags(dbContext, product, tags.Select(tag => tag.Id).ToHashSet());
        }

        await EnsureFaqsAsync(dbContext);
        await dbContext.SaveChangesAsync();
    }

    private static void ApplyChatbotBuilderDefaults(Product product, int categoryId)
    {
        product.ProductCategoryId = categoryId;
        product.Name = "Chatbot Builder";
        product.PersianTitle = "چتبات‌ساز هوش‌یاران";
        product.ShortDescription = "بستری برای ساخت و انتشار چتبات‌های اختصاصی سازمانی که می‌توانند با ترکیب ایجنت‌های RAG، دیتابیس، جست‌وجوی وب، Planner، Ranker و Cache به اسناد، داده‌ها و فرایندهای واقعی سازمان متصل شوند.";
        product.LongDescription = ChatbotBuilderLongDescription;
        product.ProblemsSolved = string.Join('\n',
        [
            "وابستگی به پروژه‌های نرم‌افزاری جداگانه برای ساخت هر چتبات جدید",
            "پراکنده بودن دانش سازمانی در فایل‌ها، مستندات، دستورالعمل‌ها و پایگاه‌های داده",
            "پاسخ‌گویی کند و تکراری تیم‌های پشتیبانی، منابع انسانی، فروش، عملیات و فناوری اطلاعات",
            "نبود کنترل کافی روی مدل پایه، منابع پاسخ، سطح دسترسی و کیفیت خروجی چتبات‌ها",
            "سختی اتصال هم‌زمان چتبات به اسناد، دیتابیس‌ها و منابع آنلاین",
            "نبود سازوکار رتبه‌بندی، ترکیب پاسخ و کش برای استفاده سازمانی در مقیاس بالا"
        ]);
        product.Benefits = string.Join('\n',
        [
            "ساخت سریع چتبات‌های اختصاصی بدون شروع پروژه نرم‌افزاری از صفر",
            "استفاده از چند ایجنت تخصصی در یک چتبات واحد",
            "اتصال چتبات به اسناد، فایل‌ها، دستورالعمل‌ها، دیتابیس‌ها و منابع وب",
            "کنترل معماری پاسخ با Planner، Ranker، Merger و Cache",
            "کاهش هزینه پاسخ‌گویی و افزایش سرعت دسترسی کارکنان و مشتریان به اطلاعات",
            "امکان انتشار داخلی، عمومی یا تعبیه در سامانه‌ها و وب‌سایت‌های دیگر"
        ]);
        product.PublicFeatures = string.Join('\n',
        [
            "ساخت و مدیریت چند چتبات در پورتال سازمانی",
            "تعریف ایجنت‌های RAG برای استفاده از فایل‌ها، مستندات و دانش داخلی",
            "تعریف ایجنت‌های SQL/Data برای پاسخ‌گویی بر اساس داده‌های ساختاریافته",
            "تعریف ایجنت Search برای تکمیل پاسخ با جست‌وجوی اینترنتی",
            "انتخاب مدل پایه، تنظیم Planner، Ranker و Cache برای هر چتبات",
            "اجرای ترتیبی یا موازی ایجنت‌ها بر اساس نوع درخواست",
            "مدیریت سطح دسترسی کاربران داخلی و خارجی",
            "انتشار چتبات روی وب‌سایت، پورتال داخلی یا سامانه‌های دیگر"
        ]);
        product.TargetAudience = string.Join('\n',
        [
            "سازمان‌هایی که مستندات، دستورالعمل‌ها و دانش داخلی قابل جست‌وجو دارند",
            "شرکت‌هایی که می‌خواهند چتبات اختصاصی برای مشتریان یا کارکنان خود بسازند",
            "تیم‌های فناوری اطلاعات، تحول دیجیتال، داده و هوش مصنوعی",
            "مدیران منابع انسانی، پشتیبانی، فروش، عملیات و آموزش",
            "سازمان‌هایی که به AI خصوصی، کنترل داده و استقرار قابل مدیریت نیاز دارند"
        ]);
        product.UseCases = string.Join('\n',
        [
            "چتبات داخلی برای پاسخ‌گویی به کارکنان درباره فرایندها، آیین‌نامه‌ها و سامانه‌ها",
            "چتبات پشتیبانی مشتریان متصل به مستندات محصول، SLA و سوابق خدمات",
            "دستیار فروش برای پاسخ‌گویی بر اساس کاتالوگ، قیمت‌نامه، قرارداد و شرایط همکاری",
            "دستیار منابع انسانی برای مرخصی، مزایا، آموزش، استخدام و پرسش‌های پرتکرار",
            "چتبات فنی برای جست‌وجو در راهنماها، دستورالعمل‌های عملیاتی و دانش تعمیرات",
            "دستیار مدیریتی متصل به دیتابیس‌ها و گزارش‌های تحلیلی سازمان",
            "چتبات وب‌سایت برای پاسخ‌گویی هوشمند به بازدیدکنندگان و هدایت سرنخ‌های فروش",
            "پورتال دانشی سازمان برای کاهش بار پاسخ‌گویی واحدهای داخلی"
        ]);
        product.HeroImagePath = ChatbotBuilderHeroUrl;
        product.LogoPath = "/uploads/media/imported/brand/hooshyaran-logo-small.png";
        product.CtaText = "درخواست دمو چتبات‌ساز";
        product.IsFeatured = true;
        product.SortOrder = 35;
        product.IsActive = true;
        product.SeoTitle = "چتبات‌ساز سازمانی هوش‌یاران | ساخت چتبات اختصاصی با RAG و ایجنت هوشمند";
        product.SeoDescription = "چتبات‌ساز هوش‌یاران بستری برای ساخت چتبات‌های سازمانی متصل به اسناد، دیتابیس و وب با ایجنت‌های RAG، SQL، Planner، Ranker و Cache است.";
        product.SeoKeywords = "چتبات ساز, چتبات سازمانی, ساخت چتبات اختصاصی, چتبات مبتنی بر RAG, چتبات هوش مصنوعی, ایجنت هوش مصنوعی, چتبات متصل به دیتابیس, چتبات فارسی سازمانی, هوش مصنوعی سازمانی";
    }

    private static async Task<ProductCategory> EnsureCategoryAsync(HooshyaranDbContext dbContext)
    {
        const string slug = "enterprise-llm-apps";
        var category = await dbContext.ProductCategories.SingleOrDefaultAsync(item => item.Slug == slug);
        if (category is null)
        {
            category = new ProductCategory { Slug = slug };
            dbContext.ProductCategories.Add(category);
        }

        category.Title = "اپلیکیشن‌های LLM سازمانی";
        category.Description = "محصولات کاربردی برای ساخت دستیارها، چتبات‌ها و اپلیکیشن‌های هوشمند متصل به داده و فرایندهای سازمان";
        category.SortOrder = 25;
        category.IsActive = true;

        await dbContext.SaveChangesAsync();
        return category;
    }

    private static async Task EnsureMediaAssetAsync(HooshyaranDbContext dbContext, DateTimeOffset now)
    {
        await EnsureMediaAssetAsync(
            dbContext,
            ChatbotBuilderHeroUrl,
            "تصویر شاخص چتبات‌ساز هوش‌یاران",
            "تصویر معماری چتبات‌ساز سازمانی با ایجنت‌های متصل به اسناد، دیتابیس، جست‌وجو، امنیت و کش",
            "چتبات‌ساز سازمانی هوش‌یاران",
            "تصویر شاخص محصول چتبات‌ساز هوش‌یاران برای معرفی ساخت چتبات‌های اختصاصی سازمانی با معماری چندایجنتی.",
            "تصویر شاخص محصول چتبات‌ساز سازمانی هوش‌یاران با نمایش اتصال چتبات به اسناد، دیتابیس، وب و لایه‌های هوشمند.",
            now);

        await EnsureMediaAssetAsync(
            dbContext,
            ChatbotBuilderArchitectureUrl,
            "معماری ایجنت‌های فروش در چتبات‌ساز هوش‌یاران",
            "نمونه معماری ایجنتیک چتبات‌ساز هوش‌یاران با ایجنت پلنر، ایجنت فروش، قوانین فروش، CRM، سفارش، موجودی، فاکتور، RAG، جست‌وجو، مرجر، رنکر، کش و پاسخ نهایی",
            "نمونه اتصال ایجنت‌های فروش در چتبات‌ساز",
            "تصویر مفهومی اتصال ایجنت‌های واقعی فروش در چتبات‌ساز هوش‌یاران، از پلنر تا مرجر، رنکر، کش و پاسخ نهایی چتبات.",
            "تصویر معماری ایجنتیک چتبات سازمانی برای فروش با ایجنت‌های فروش، قوانین فروش، CRM، سفارش، موجودی، فاکتور، RAG و جست‌وجو.",
            now);
    }

    private static async Task EnsureMediaAssetAsync(
        HooshyaranDbContext dbContext,
        string url,
        string name,
        string altText,
        string title,
        string description,
        string seoDescription,
        DateTimeOffset now)
    {
        var asset = await dbContext.MediaAssets.SingleOrDefaultAsync(item => item.Url == url);
        if (asset is null)
        {
            asset = new MediaAsset
            {
                Url = url,
                CreatedAt = now,
                Name = name,
                AltText = altText,
                Title = title,
                Description = description,
                SeoDescription = seoDescription,
                UpdatedAt = now
            };
            dbContext.MediaAssets.Add(asset);
        }
    }

    private static async Task<Tag> EnsureTagAsync(
        HooshyaranDbContext dbContext,
        string name,
        string slug,
        string description,
        DateTimeOffset now)
    {
        var tag = await dbContext.Tags.SingleOrDefaultAsync(item => item.Slug == slug);
        if (tag is null)
        {
            tag = new Tag
            {
                Slug = slug,
                CreatedAt = now,
                Name = name,
                Description = description,
                UpdatedAt = now
            };
            dbContext.Tags.Add(tag);
        }
        return tag;
    }

    private static void SyncProductTags(HooshyaranDbContext dbContext, Product product, HashSet<int> selectedTagIds)
    {
        var currentTagIds = product.ProductTags.Select(item => item.TagId).ToHashSet();

        foreach (var relation in product.ProductTags.Where(item => !selectedTagIds.Contains(item.TagId)).ToList())
        {
            dbContext.ProductTags.Remove(relation);
        }

        foreach (var tagId in selectedTagIds.Except(currentTagIds))
        {
            product.ProductTags.Add(new ProductTag { ProductId = product.Id, TagId = tagId });
        }
    }

    private static async Task EnsureFaqsAsync(HooshyaranDbContext dbContext)
    {
        var currentFaqs = await dbContext.FaqItems
            .Where(item => item.PageKey == ChatbotBuilderPageKey)
            .ToListAsync();
        if (currentFaqs.Any())
        {
            return;
        }

        dbContext.FaqItems.AddRange(
            new FaqItem
            {
                PageKey = ChatbotBuilderPageKey,
                SortOrder = 10,
                IsActive = true,
                Question = "چتبات‌ساز هوش‌یاران چه تفاوتی با یک چتبات ساده دارد؟",
                Answer = "این محصول فقط یک پنجره گفت‌وگو نیست. هر چتبات می‌تواند چند ایجنت تخصصی داشته باشد، سوال را تحلیل کند، منابع مختلف را هم‌زمان بررسی کند، پاسخ‌ها را ترکیب و رتبه‌بندی کند و نتیجه را برای استفاده سریع‌تر در دفعات بعدی کش کند."
            },
            new FaqItem
            {
                PageKey = ChatbotBuilderPageKey,
                SortOrder = 20,
                IsActive = true,
                Question = "آیا چتبات می‌تواند به اسناد داخلی سازمان وصل شود؟",
                Answer = "بله. با ایجنت RAG می‌توان چتبات را به فایل‌ها، مستندات، دستورالعمل‌ها، راهنماها و دانش داخلی سازمان متصل کرد تا پاسخ‌ها بر اساس منابع واقعی سازمان تولید شوند."
            },
            new FaqItem
            {
                PageKey = ChatbotBuilderPageKey,
                SortOrder = 30,
                IsActive = true,
                Question = "آیا امکان اتصال چتبات به دیتابیس وجود دارد؟",
                Answer = "بله. ایجنت SQL یا Data برای سناریوهایی طراحی شده که پاسخ باید از داده‌های ساختاریافته، گزارش‌ها یا پایگاه‌های داده سازمان استخراج شود."
            },
            new FaqItem
            {
                PageKey = ChatbotBuilderPageKey,
                SortOrder = 40,
                IsActive = true,
                Question = "چتبات کجا قابل استفاده است؟",
                Answer = "چتبات می‌تواند در پورتال داخلی سازمان، وب‌سایت عمومی، سامانه‌های سازمانی یا از طریق اسکریپت و API در محصولات دیگر استفاده شود."
            },
            new FaqItem
            {
                PageKey = ChatbotBuilderPageKey,
                SortOrder = 50,
                IsActive = true,
                Question = "آیا می‌توان دسترسی کاربران و منابع پاسخ را کنترل کرد؟",
                Answer = "بله. چتبات‌ساز برای استفاده سازمانی طراحی شده و امکان تعریف سطح دسترسی، انتشار داخلی یا عمومی و مدیریت منابع قابل استفاده برای هر چتبات را فراهم می‌کند."
            });
    }

    private const string ChatbotBuilderLongDescription = """
<p>چتبات‌ساز هوش‌یاران برای سازمان‌هایی طراحی شده که می‌خواهند دانش داخلی، مستندات، دستورالعمل‌ها، داده‌ها و فرایندهای خود را به یک دستیار گفت‌وگومحور قابل استفاده تبدیل کنند؛ بدون اینکه برای هر سناریو وارد یک پروژه نرم‌افزاری جداگانه و زمان‌بر شوند.</p>
<p>در این محصول، هر چتبات می‌تواند از یک یا چند ایجنت تخصصی استفاده کند. ایجنت RAG برای پاسخ‌گویی بر اساس اسناد و فایل‌های سازمان، ایجنت SQL یا Data برای پرس‌وجو از پایگاه‌های داده، و ایجنت جست‌وجوی وب برای تکمیل پاسخ با اطلاعات آنلاین قابل استفاده است. بالای این ایجنت‌ها، یک ایجنت Planner قرار می‌گیرد که سوال کاربر را تحلیل می‌کند و تصمیم می‌گیرد کدام ایجنت‌ها باید اجرا شوند، اجرای آن‌ها به‌صورت ترتیبی یا موازی انجام شود و خروجی‌ها چگونه ترکیب شوند.</p>
<p>پس از اجرای ایجنت‌ها، پاسخ‌ها به لایه Merger و Ranker منتقل می‌شوند تا بهترین پاسخ‌ها انتخاب، ترکیب و رتبه‌بندی شوند. سپس نتیجه نهایی می‌تواند در لایه Cache ذخیره شود تا در درخواست‌های بعدی، پاسخ‌های پرتکرار سریع‌تر و با مصرف کمتر تولید شوند. این معماری باعث می‌شود چتبات فقط یک رابط ساده گفت‌وگو نباشد، بلکه یک سیستم تصمیم‌گیر و هماهنگ‌کننده میان منابع دانشی، داده‌ای و عملیاتی سازمان باشد.</p>
<h3>چرا سازمان‌ها به چتبات‌ساز نیاز دارند؟</h3>
<p>در بسیاری از شرکت‌ها، دانش مهم سازمان در فایل‌های PDF، اکسل، ورد، سامانه‌های داخلی، پایگاه‌های داده، پیام‌های تیمی و تجربه کارشناسان پخش شده است. نتیجه این است که کارکنان و مشتریان برای رسیدن به پاسخ درست باید از چند نفر بپرسند، چند سامانه را جست‌وجو کنند یا منتظر پاسخ تیم پشتیبانی بمانند. چتبات‌ساز هوش‌یاران این دانش پراکنده را به چتبات‌های قابل کنترل تبدیل می‌کند تا هر واحد سازمانی بتواند دستیار اختصاصی خود را داشته باشد.</p>
<h3>مثال‌هایی از کاربرد در صنایع مختلف</h3>
<table>
    <thead>
        <tr>
            <th>صنعت</th>
            <th>نمونه چتبات قابل ساخت</th>
            <th>ارزش مدیریتی</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>بانک و خدمات مالی</td>
            <td>چتبات پاسخ‌گوی قوانین اعتباری، شرایط تسهیلات، فرایندهای شعب و پرسش‌های پرتکرار مشتریان</td>
            <td>کاهش فشار روی شعب و مرکز تماس، یکپارچگی پاسخ‌ها و کنترل منابع دانشی حساس</td>
        </tr>
        <tr>
            <td>بیمه</td>
            <td>چتبات راهنمای صدور بیمه‌نامه، خسارت، مدارک مورد نیاز و شرایط پوشش‌ها</td>
            <td>افزایش سرعت پاسخ‌گویی نمایندگان و کاهش خطای انسانی در توضیح شرایط بیمه</td>
        </tr>
        <tr>
            <td>تولید و صنعت</td>
            <td>دستیار فنی برای دستورالعمل‌های تعمیرات، نگهداری، ایمنی و راهنمای اپراتورها</td>
            <td>دسترسی سریع‌تر به دانش عملیاتی و کاهش توقف‌های ناشی از جست‌وجوی اطلاعات</td>
        </tr>
        <tr>
            <td>دولت و خدمات عمومی</td>
            <td>چتبات پاسخ‌گوی آیین‌نامه‌ها، خدمات شهروندی، مراحل ثبت درخواست و پیگیری پرونده</td>
            <td>کاهش مراجعات تکراری، شفافیت بیشتر و پاسخ‌گویی یکنواخت در کانال‌های مختلف</td>
        </tr>
        <tr>
            <td>سلامت و درمان</td>
            <td>دستیار داخلی برای پروتکل‌ها، مسیر ارجاع، راهنمای بیماران و دستورالعمل‌های اداری</td>
            <td>کاهش زمان دسترسی کارکنان به اطلاعات و کمک به استانداردسازی فرایندهای پاسخ‌گویی</td>
        </tr>
        <tr>
            <td>نفت، گاز و انرژی</td>
            <td>چتبات عملیاتی برای دستورالعمل‌های ایمنی، تجهیزات، گزارش رخداد و دانش فنی پروژه‌ها</td>
            <td>حفظ دانش تخصصی، پاسخ‌گویی سریع در محیط‌های عملیاتی و کاهش وابستگی به افراد کلیدی</td>
        </tr>
        <tr>
            <td>آموزش و دانشگاه</td>
            <td>چتبات راهنمای دانشجو، آیین‌نامه آموزشی، برنامه‌ها، منابع درسی و خدمات اداری</td>
            <td>کاهش پرسش‌های تکراری، بهبود تجربه دانشجو و آزاد شدن زمان تیم‌های آموزشی</td>
        </tr>
    </tbody>
</table>
<h3>مثال‌هایی از کاربرد در واحدهای کسب‌وکار</h3>
<ul>
    <li><strong>منابع انسانی:</strong> چتباتی برای پاسخ به سوالات کارکنان درباره مرخصی، مزایا، بیمه، آموزش، استخدام، ارزیابی عملکرد و آیین‌نامه‌های داخلی.</li>
    <li><strong>پشتیبانی مشتریان:</strong> چتباتی متصل به مستندات محصول، راهنماهای رفع خطا، SLA و سوالات پرتکرار که قبل از ارجاع به کارشناس، پاسخ اولیه دقیق ارائه می‌دهد.</li>
    <li><strong>فروش و بازاریابی:</strong> دستیار فروش برای توضیح محصولات، مقایسه پلن‌ها، آماده‌سازی پاسخ به مشتری، تولید متن پیشنهادی و هدایت سرنخ‌ها به تیم فروش.</li>
    <li><strong>فناوری اطلاعات:</strong> چتبات Service Desk برای راهنمای سامانه‌های داخلی، دسترسی‌ها، خطاهای رایج، سیاست‌های امنیتی و درخواست‌های پرتکرار کاربران.</li>
    <li><strong>حقوقی و قراردادها:</strong> دستیار جست‌وجوی بندهای قراردادی، رویه‌های داخلی، چک‌لیست‌های بررسی و پاسخ به سوالات پرتکرار واحدهای سازمان.</li>
    <li><strong>عملیات و زنجیره تامین:</strong> چتباتی برای پرس‌وجو درباره دستورالعمل‌ها، وضعیت سفارش، انبار، تامین‌کنندگان، رویه‌های کنترل کیفیت و گزارش‌های عملیاتی.</li>
</ul>
<h3>ساخت چتبات در پورتال</h3>
<p>کاربر در پورتال چتبات‌ساز می‌تواند چتبات جدید ایجاد کند، نام و توضیح آن را بنویسد، مدل پایه را انتخاب کند، ایجنت‌های آماده را به چتبات متصل کند، تنظیمات Planner، Ranker و Cache را مشخص کند و سطح دسترسی چتبات را تعیین کند. چتبات می‌تواند فقط برای کاربران داخلی سازمان فعال شود، روی وب‌سایت منتشر شود یا از طریق اسکریپت و API در سامانه‌های دیگر قرار بگیرد.</p>
<h3>معماری چندایجنتی قابل کنترل</h3>
<p>مزیت اصلی چتبات‌ساز هوش‌یاران در معماری چندایجنتی آن است. به‌جای اینکه همه سوال‌ها به یک مسیر ثابت بروند، Planner نوع درخواست را تشخیص می‌دهد و تصمیم می‌گیرد پاسخ باید از سند، دیتابیس، وب یا ترکیبی از چند منبع ساخته شود. سپس Merger و Ranker خروجی‌ها را ترکیب و اولویت‌بندی می‌کنند تا پاسخ نهایی هم کامل‌تر باشد و هم با سیاست‌های سازمانی سازگار بماند.</p>
""";
}
