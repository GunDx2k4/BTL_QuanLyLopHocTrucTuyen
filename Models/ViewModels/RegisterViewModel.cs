using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100)]
    public required string FullName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public required string ConfirmPassword { get; set; }
}
