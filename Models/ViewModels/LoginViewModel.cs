using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
