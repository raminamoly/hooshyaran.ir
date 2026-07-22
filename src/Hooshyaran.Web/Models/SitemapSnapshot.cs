using System.ComponentModel.DataAnnotations;

namespace Hooshyaran.Web.Models;

public class SitemapSnapshot
{
    public int Id { get; set; }

    public string Xml { get; set; } = string.Empty;

    public int UrlCount { get; set; }

    [MaxLength(260)]
    public string BaseUrl { get; set; } = "https://hooshyaran.ir";

    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
