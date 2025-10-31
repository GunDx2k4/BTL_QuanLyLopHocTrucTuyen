using System;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class ConverterHelper
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    public static Guid GetSessionId(this ClaimsPrincipal user)
    {
        var sessionIdClaim = user.FindFirst("SessionId")?.Value;
        return sessionIdClaim != null ? Guid.Parse(sessionIdClaim) : Guid.Empty;
    }

    public static bool TryParseIsoWeek(string input, out DateTime startOfWeek)
    {
        startOfWeek = default;
        if (!Regex.IsMatch(input, @"^\d{4}-W\d{2}$"))
            return false;

        // Kiểm tra đúng dạng YYYY-Www
        var parts = input.Split("-W");
        int year = int.Parse(parts[0]);
        int week = int.Parse(parts[1]);

        // Xác định ngày đầu tiên của năm
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

        // ISO 8601: tuần 1 là tuần chứa Thứ Năm đầu tiên của năm
        DateTime firstThursday = jan1.AddDays(daysOffset);

        int firstWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            firstThursday,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday
        );

        int weekDiff = week - firstWeek;
        DateTime result = firstThursday.AddDays(weekDiff * 7);

        // Lấy ngày thứ Hai đầu tuần
        startOfWeek = result.AddDays(-3);
        return true;
    }


}
