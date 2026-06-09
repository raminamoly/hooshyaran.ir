namespace Hooshyaran.Web.Models;

public class StaticPageTag
{
    public int StaticPageId { get; set; }

    public StaticPage? StaticPage { get; set; }

    public int TagId { get; set; }

    public Tag? Tag { get; set; }
}
