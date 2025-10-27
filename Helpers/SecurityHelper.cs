using System;
using System.Security.Cryptography;
using System.Text;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class SecurityHelper
{
    public static string HashPassword(string password)
    {
        return Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(password)));
    }
}
