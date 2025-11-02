using System;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

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

    public static bool TryParseIsoWeek(string input, out DateTime monday)
    {
        monday = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Dạng "2025-W44"
        var parts = input.Split("-W");
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out int year)) return false;
        if (!int.TryParse(parts[1], out int week)) return false;

        try
        {
            monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string ToEnrollmentStatusString(this EnrollmentStatus status)
    {
        return status switch
        {
                EnrollmentStatus.Enrolled => "Đã đăng ký",
                EnrollmentStatus.Completed => "Đã hoàn thành",
                EnrollmentStatus.Dropped => "Đã rút khỏi khóa học",
                _ => status.ToString()
        };
    }

}
