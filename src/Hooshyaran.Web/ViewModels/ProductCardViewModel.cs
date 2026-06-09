namespace Hooshyaran.Web.ViewModels;

public record ProductCardViewModel(
    string Title,
    string Description,
    string Category,
    string Url,
    string CtaText,
    IReadOnlyList<string> Benefits,
    string? ImageUrl = null);
