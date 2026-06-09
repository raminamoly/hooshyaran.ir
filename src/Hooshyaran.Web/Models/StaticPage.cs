namespace Hooshyaran.Web.Models;

public class StaticPage
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    public string SeoTitle { get; set; } = string.Empty;

    public string SeoDescription { get; set; } = string.Empty;

    public string SeoKeywords { get; set; } = string.Empty;

    public ICollection<StaticPageTag> StaticPageTags { get; set; } = [];
}
