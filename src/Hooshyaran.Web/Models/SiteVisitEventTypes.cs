namespace Hooshyaran.Web.Models;

public static class SiteVisitEventTypes
{
    public const string PageView = "PageView";

    public const string ArticleClick = "ArticleClick";

    public const string DemoSubmit = "DemoSubmit";

    public static string ToPersian(string eventType) => eventType switch
    {
        ArticleClick => "کلیک مقاله",
        DemoSubmit => "ثبت فرم دمو",
        _ => "بازدید صفحه"
    };
}
