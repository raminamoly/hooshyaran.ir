using System.Globalization;

namespace Hooshyaran.Web.Utilities;

public static class PersianDateFormatter
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly TimeSpan TehranOffset = TimeSpan.FromHours(3.5);

    public static string ToPersianDate(DateTimeOffset value)
    {
        var tehranDate = value.ToOffset(TehranOffset).DateTime;
        var year = Calendar.GetYear(tehranDate);
        var month = Calendar.GetMonth(tehranDate);
        var day = Calendar.GetDayOfMonth(tehranDate);

        return ToPersianDigits($"{year:0000}/{month:00}/{day:00}");
    }

    public static string ToPersianDateTime(DateTimeOffset value)
    {
        var tehranDate = value.ToOffset(TehranOffset).DateTime;
        var time = ToPersianDigits($"{tehranDate:HH:mm}");

        return $"{ToPersianDate(value)} - {time}";
    }

    public static string CurrentPersianYear()
    {
        var tehranDate = DateTimeOffset.UtcNow.ToOffset(TehranOffset).DateTime;

        return ToPersianDigits(Calendar.GetYear(tehranDate).ToString("0000"));
    }

    public static string ToPersianDigits(string value)
    {
        return value
            .Replace('0', '۰')
            .Replace('1', '۱')
            .Replace('2', '۲')
            .Replace('3', '۳')
            .Replace('4', '۴')
            .Replace('5', '۵')
            .Replace('6', '۶')
            .Replace('7', '۷')
            .Replace('8', '۸')
            .Replace('9', '۹');
    }
}
