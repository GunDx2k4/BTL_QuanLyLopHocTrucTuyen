using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class EnrollViewModel
{
    public Guid CourseId { get; set; }

    public Guid UserId { get; set; }
}
