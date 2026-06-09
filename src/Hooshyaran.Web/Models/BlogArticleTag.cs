namespace Hooshyaran.Web.Models;

public class BlogArticleTag
{
    public int BlogArticleId { get; set; }

    public BlogArticle? BlogArticle { get; set; }

    public int TagId { get; set; }

    public Tag? Tag { get; set; }
}
