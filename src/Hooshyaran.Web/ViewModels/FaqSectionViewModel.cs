namespace Hooshyaran.Web.ViewModels;

public record FaqSectionViewModel(string Title, IReadOnlyList<FaqItemViewModel> Items);

public record FaqItemViewModel(string Question, string Answer);
