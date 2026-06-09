namespace Hooshyaran.Web.ViewModels;

public record PageHeroViewModel(
    string Eyebrow,
    string Title,
    string Description,
    string? PrimaryText = null,
    string? PrimaryUrl = null,
    string? SecondaryText = null,
    string? SecondaryUrl = null,
    string? ImageUrl = null,
    string? ImageAlt = null,
    string? CssClass = null);
