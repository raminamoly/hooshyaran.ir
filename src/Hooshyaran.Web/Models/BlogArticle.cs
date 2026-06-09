namespace Hooshyaran.Web.Models;

public class BlogArticle
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;

    public int? AdminUserId { get; set; }

    public AdminUser? AdminUser { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public string SeoTitle { get; set; } = string.Empty;

    public string SeoDescription { get; set; } = string.Empty;

    public string SeoKeywords { get; set; } = string.Empty;

    public ICollection<BlogArticleTag> BlogArticleTags { get; set; } = [];
}
