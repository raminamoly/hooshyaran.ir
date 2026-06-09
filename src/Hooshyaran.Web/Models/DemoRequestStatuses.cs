namespace Hooshyaran.Web.Models;

public static class DemoRequestStatuses
{
    public const string New = "New";

    public const string Reviewed = "Reviewed";

    public const string Contacted = "Contacted";

    public const string Closed = "Closed";

    public static string ToPersian(string status) => status switch
    {
        Reviewed => "بررسی‌شده",
        Contacted => "تماس گرفته شد",
        Closed => "بسته شد",
        _ => "جدید"
    };
}
