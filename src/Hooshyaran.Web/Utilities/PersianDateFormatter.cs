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

    public static string ToPersianDateTimeInput(DateTimeOffset value)
    {
        var tehranDate = value.ToOffset(TehranOffset).DateTime;
        var year = Calendar.GetYear(tehranDate);
        var month = Calendar.GetMonth(tehranDate);
        var day = Calendar.GetDayOfMonth(tehranDate);

        return $"{year:0000}/{month:00}/{day:00} {tehranDate:HH:mm}";
    }

    public static bool TryParsePersianDateTime(string? value, out DateTimeOffset result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = ToEnglishDigits(value)
            .Replace('-', '/')
            .Replace(" - ", " ")
            .Trim();
        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2)
        {
            return false;
        }

        var dateParts = parts[0].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (dateParts.Length != 3 ||
            !int.TryParse(dateParts[0], out var year) ||
            !int.TryParse(dateParts[1], out var month) ||
            !int.TryParse(dateParts[2], out var day))
        {
            return false;
        }

        var hour = 0;
        var minute = 0;
        if (parts.Length == 2)
        {
            var timeParts = parts[1].Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out hour) ||
                !int.TryParse(timeParts[1], out minute))
            {
                return false;
            }
        }

        if (hour is < 0 or > 23 || minute is < 0 or > 59)
        {
            return false;
        }

        try
        {
            var tehranDateTime = Calendar.ToDateTime(year, month, day, hour, minute, 0, 0);
            result = new DateTimeOffset(tehranDateTime, TehranOffset);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static int CurrentPersianYearNumber()
    {
        var tehranDate = DateTimeOffset.UtcNow.ToOffset(TehranOffset).DateTime;

        return Calendar.GetYear(tehranDate);
    }

    public static string CurrentPersianYear()
    {
        return ToPersianDigits(CurrentPersianYearNumber().ToString("0000"));
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

    public static string ToEnglishDigits(string value)
    {
        return value
            .Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9')
            .Replace('٠', '0')
            .Replace('١', '1')
            .Replace('٢', '2')
            .Replace('٣', '3')
            .Replace('٤', '4')
            .Replace('٥', '5')
            .Replace('٦', '6')
            .Replace('٧', '7')
            .Replace('٨', '8')
            .Replace('٩', '9');
    }
}
