using Hooshyaran.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public class HooshyaranDbContext(DbContextOptions<HooshyaranDbContext> options) : DbContext(options)
{
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<BlogArticle> BlogArticles => Set<BlogArticle>();

    public DbSet<StaticPage> StaticPages => Set<StaticPage>();

    public DbSet<FaqItem> FaqItems => Set<FaqItem>();

    public DbSet<CtaBlock> CtaBlocks => Set<CtaBlock>();

    public DbSet<SeoMetadata> SeoMetadata => Set<SeoMetadata>();

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<BlogArticleTag> BlogArticleTags => Set<BlogArticleTag>();

    public DbSet<ProductTag> ProductTags => Set<ProductTag>();

    public DbSet<StaticPageTag> StaticPageTags => Set<StaticPageTag>();

    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    public DbSet<DemoRequest> DemoRequests => Set<DemoRequest>();

    public DbSet<SiteVisitLog> SiteVisitLogs => Set<SiteVisitLog>();

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.Property(category => category.Title).HasMaxLength(160).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(120).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(600);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.PersianTitle).HasMaxLength(220).IsRequired();
            entity.Property(product => product.Slug).HasMaxLength(120).IsRequired();
            entity.Property(product => product.ShortDescription).HasMaxLength(600);
            entity.Property(product => product.HeroImagePath).HasMaxLength(260);
            entity.Property(product => product.LogoPath).HasMaxLength(260);
            entity.Property(product => product.CtaText).HasMaxLength(120);
            entity.Property(product => product.SeoTitle).HasMaxLength(220);
            entity.Property(product => product.SeoDescription).HasMaxLength(320);
            entity.Property(product => product.SeoKeywords).HasMaxLength(500);

            entity.HasOne(product => product.ProductCategory)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BlogArticle>(entity =>
        {
            entity.HasIndex(article => article.Slug).IsUnique();
            entity.Property(article => article.Title).HasMaxLength(220).IsRequired();
            entity.Property(article => article.Slug).HasMaxLength(160).IsRequired();
            entity.Property(article => article.Summary).HasMaxLength(700);
            entity.Property(article => article.ImagePath).HasMaxLength(260);
            entity.Property(article => article.AuthorName).HasMaxLength(120);
            entity.Property(article => article.SeoTitle).HasMaxLength(220);
            entity.Property(article => article.SeoDescription).HasMaxLength(320);
            entity.Property(article => article.SeoKeywords).HasMaxLength(500);

            entity.HasOne(article => article.AdminUser)
                .WithMany(user => user.BlogArticles)
                .HasForeignKey(article => article.AdminUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StaticPage>(entity =>
        {
            entity.HasIndex(page => page.Key).IsUnique();
            entity.HasIndex(page => page.Slug).IsUnique();
            entity.Property(page => page.Key).HasMaxLength(120).IsRequired();
            entity.Property(page => page.Title).HasMaxLength(220).IsRequired();
            entity.Property(page => page.Slug).HasMaxLength(160).IsRequired();
            entity.Property(page => page.Summary).HasMaxLength(700);
            entity.Property(page => page.SeoTitle).HasMaxLength(220);
            entity.Property(page => page.SeoDescription).HasMaxLength(320);
            entity.Property(page => page.SeoKeywords).HasMaxLength(500);
        });

        modelBuilder.Entity<FaqItem>(entity =>
        {
            entity.HasIndex(faq => new { faq.PageKey, faq.SortOrder });
            entity.Property(faq => faq.Question).HasMaxLength(260).IsRequired();
            entity.Property(faq => faq.Answer).HasMaxLength(1200).IsRequired();
            entity.Property(faq => faq.PageKey).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<CtaBlock>(entity =>
        {
            entity.HasIndex(cta => cta.Key).IsUnique();
            entity.Property(cta => cta.Key).HasMaxLength(120).IsRequired();
            entity.Property(cta => cta.Title).HasMaxLength(220).IsRequired();
            entity.Property(cta => cta.Description).HasMaxLength(700);
            entity.Property(cta => cta.ButtonText).HasMaxLength(120);
            entity.Property(cta => cta.ButtonUrl).HasMaxLength(260);
        });

        modelBuilder.Entity<SeoMetadata>(entity =>
        {
            entity.HasIndex(seo => seo.PageKey).IsUnique();
            entity.Property(seo => seo.PageKey).HasMaxLength(120).IsRequired();
            entity.Property(seo => seo.Title).HasMaxLength(220).IsRequired();
            entity.Property(seo => seo.Description).HasMaxLength(320);
            entity.Property(seo => seo.Keywords).HasMaxLength(500);
            entity.Property(seo => seo.CanonicalPath).HasMaxLength(260);
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(user => user.UserName).IsUnique();
            entity.HasIndex(user => user.Email);
            entity.Property(user => user.UserName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(160);
            entity.Property(user => user.Email).HasMaxLength(180);
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(60).IsRequired();
        });

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.Property(settings => settings.WebsiteUrl).HasMaxLength(260);
            entity.Property(settings => settings.SmtpHost).HasMaxLength(180);
            entity.Property(settings => settings.SmtpUserName).HasMaxLength(180);
            entity.Property(settings => settings.SmtpPassword).HasMaxLength(500);
            entity.Property(settings => settings.FromEmail).HasMaxLength(180);
            entity.Property(settings => settings.FromName).HasMaxLength(180);
            entity.Property(settings => settings.AdminNotificationEmail).HasMaxLength(180);
        });

        modelBuilder.Entity<DemoRequest>(entity =>
        {
            entity.HasIndex(request => request.CreatedAt);
            entity.HasIndex(request => request.Status);
            entity.Property(request => request.FullName).HasMaxLength(160).IsRequired();
            entity.Property(request => request.OrganizationName).HasMaxLength(180).IsRequired();
            entity.Property(request => request.JobTitle).HasMaxLength(160);
            entity.Property(request => request.PhoneNumber).HasMaxLength(80).IsRequired();
            entity.Property(request => request.Email).HasMaxLength(180);
            entity.Property(request => request.NeedArea).HasMaxLength(220);
            entity.Property(request => request.PreferredTime).HasMaxLength(160);
            entity.Property(request => request.Notes).HasMaxLength(1400);
            entity.Property(request => request.Status).HasMaxLength(60).IsRequired();
            entity.Property(request => request.AdminNotes).HasMaxLength(1400);
        });

        modelBuilder.Entity<SiteVisitLog>(entity =>
        {
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => log.EventType);
            entity.HasIndex(log => log.VisitorKey);
            entity.HasIndex(log => log.BlogArticleId);
            entity.Property(log => log.VisitorKey).HasMaxLength(64).IsRequired();
            entity.Property(log => log.EventType).HasMaxLength(60).IsRequired();
            entity.Property(log => log.Path).HasMaxLength(360).IsRequired();
            entity.Property(log => log.PageTitle).HasMaxLength(220);
            entity.Property(log => log.IpAddress).HasMaxLength(80);
            entity.Property(log => log.UserAgent).HasMaxLength(600);
            entity.Property(log => log.Browser).HasMaxLength(120);
            entity.Property(log => log.Device).HasMaxLength(80);
            entity.Property(log => log.Referrer).HasMaxLength(360);

            entity.HasOne(log => log.BlogArticle)
                .WithMany()
                .HasForeignKey(log => log.BlogArticleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasIndex(asset => asset.Url).IsUnique();
            entity.Property(asset => asset.Url).HasMaxLength(360).IsRequired();
            entity.Property(asset => asset.Name).HasMaxLength(260);
            entity.Property(asset => asset.AltText).HasMaxLength(220);
            entity.Property(asset => asset.Title).HasMaxLength(220);
            entity.Property(asset => asset.Description).HasMaxLength(700);
            entity.Property(asset => asset.SeoDescription).HasMaxLength(320);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(tag => tag.Name).IsUnique();
            entity.HasIndex(tag => tag.Slug).IsUnique();
            entity.Property(tag => tag.Name).HasMaxLength(120).IsRequired();
            entity.Property(tag => tag.Slug).HasMaxLength(120).IsRequired();
            entity.Property(tag => tag.Description).HasMaxLength(600);
        });

        modelBuilder.Entity<BlogArticleTag>(entity =>
        {
            entity.HasKey(item => new { item.BlogArticleId, item.TagId });
            entity.HasOne(item => item.BlogArticle)
                .WithMany(article => article.BlogArticleTags)
                .HasForeignKey(item => item.BlogArticleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Tag)
                .WithMany(tag => tag.BlogArticleTags)
                .HasForeignKey(item => item.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(item => new { item.ProductId, item.TagId });
            entity.HasOne(item => item.Product)
                .WithMany(product => product.ProductTags)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Tag)
                .WithMany(tag => tag.ProductTags)
                .HasForeignKey(item => item.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaticPageTag>(entity =>
        {
            entity.HasKey(item => new { item.StaticPageId, item.TagId });
            entity.HasOne(item => item.StaticPage)
                .WithMany(page => page.StaticPageTags)
                .HasForeignKey(item => item.StaticPageId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Tag)
                .WithMany(tag => tag.StaticPageTags)
                .HasForeignKey(item => item.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
