namespace Hooshyaran.Web.ViewModels;

public record CtaSectionViewModel(
    string Title,
    string Description,
    string PrimaryText,
    string PrimaryUrl,
    string? SecondaryText = null,
    string? SecondaryUrl = null);
