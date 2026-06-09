namespace Hooshyaran.Web.Models;

public class SeoMetadata
{
    public int Id { get; set; }

    public string PageKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Keywords { get; set; } = string.Empty;

    public string CanonicalPath { get; set; } = string.Empty;
}
