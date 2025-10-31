using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Course : Entity
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [DataType(DataType.Date)]
    public required DateTime BeginTime { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public required DateTime EndTime { get; set; }

    public ScheduleStatus Status { get; set; } = ScheduleStatus.Planned;

    [Required]
    [ForeignKey("Tenant")]
    public required Guid TenantId { get; set; }
    [JsonIgnore]
    public Tenant? Tenant { get; set; }

    [ForeignKey("Instructor")]
    [Required]
    public required Guid InstructorId { get; set; }
    [JsonIgnore]
    public User? Instructor { get; set; }

    [JsonIgnore]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    [JsonIgnore]
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

}
