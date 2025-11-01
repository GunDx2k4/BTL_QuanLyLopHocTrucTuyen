using System;
using System.ComponentModel.DataAnnotations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class RegisterTenantViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên tenant.")]
    [MaxLength(100, ErrorMessage = "Tên tenant không được vượt quá 100 ký tự.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn gói dịch vụ.")]
    public required PlanType Plan { get; set; }

    public DateTime? EndTime { get; set; }
}
