using System;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class ScheduleViewModel
{
    public required string Title { get; set; }

    public required User Instructor { get; set; }

    public required DateTime BeginTime { get; set; }

    public required DateTime EndTime { get; set; }

}
