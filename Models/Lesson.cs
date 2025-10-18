using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Lesson : Entity
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Content { get; set; }

    [Url]
    public string? VideoUrl { get; set; }

    [DataType(DataType.Date)]
    public DateTime BeginTime { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndTime { get; set; }

    [Required]
    [ForeignKey("Course")]
    public Guid CourseId { get; set; }
    [JsonIgnore]
    public Course? Course { get; set; }

    public ScheduleStatus Status { get; set; } = ScheduleStatus.Planned;

    [JsonIgnore]
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    [JsonIgnore]
    public ICollection<Material> Materials { get; set; } = new List<Material>();
}
