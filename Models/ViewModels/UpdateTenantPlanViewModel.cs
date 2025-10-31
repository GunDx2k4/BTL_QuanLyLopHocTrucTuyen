using System;
using System.ComponentModel.DataAnnotations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;

public class UpdateTenantPlanViewModel
{
    [Required(ErrorMessage = "New plan type is required.")]
    public required PlanType NewPlan { get; set; }
    
    public int DurationInMonths { get; set; }

}
