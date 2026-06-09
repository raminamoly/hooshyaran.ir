namespace Hooshyaran.Web.ViewModels;

public record FeatureGridViewModel(IReadOnlyList<FeatureItemViewModel> Items);

public record FeatureItemViewModel(string Title, string Description);
