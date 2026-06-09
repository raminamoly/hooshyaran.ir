namespace Hooshyaran.Web.Models;

public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<BlogArticleTag> BlogArticleTags { get; set; } = [];

    public ICollection<ProductTag> ProductTags { get; set; } = [];

    public ICollection<StaticPageTag> StaticPageTags { get; set; } = [];
}
