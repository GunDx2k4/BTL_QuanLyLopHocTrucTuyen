using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Enrollment : Entity
{
    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }

    [Required]
    [ForeignKey("Course")]
    public Guid CourseId { get; set; }
    [JsonIgnore]
    public Course? Course { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public double Progress { get; set; } = 0;  
    public double? FinalGrade { get; set; }
    public DateTime? DroppedAt { get; set; }
}


