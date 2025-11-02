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
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [ForeignKey("Course")]
    public required Guid CourseId { get; set; }
    [JsonIgnore]
    public Course? Course { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
}
