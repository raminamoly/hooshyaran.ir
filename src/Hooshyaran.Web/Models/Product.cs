namespace Hooshyaran.Web.Models;

public class Product
{
    public int Id { get; set; }

    public int ProductCategoryId { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PersianTitle { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string ProblemsSolved { get; set; } = string.Empty;

    public string Benefits { get; set; } = string.Empty;

    public string PublicFeatures { get; set; } = string.Empty;

    public string TargetAudience { get; set; } = string.Empty;

    public string UseCases { get; set; } = string.Empty;

    public string HeroImagePath { get; set; } = string.Empty;

    public string LogoPath { get; set; } = string.Empty;

    public string CtaText { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string SeoTitle { get; set; } = string.Empty;

    public string SeoDescription { get; set; } = string.Empty;

    public string SeoKeywords { get; set; } = string.Empty;

    public ICollection<ProductTag> ProductTags { get; set; } = [];
}
