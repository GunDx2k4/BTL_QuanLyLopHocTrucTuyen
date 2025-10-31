using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Tenant : Entity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public PlanType Plan { get; set; } = PlanType.Free;

    public DateTime? EndTime { get; set; }

    [ForeignKey("Owner")]
    [Required]
    public required Guid OwnerId { get; set; }
    [JsonIgnore]
    public User? Owner { get; set; }

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
    [JsonIgnore]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    [JsonIgnore]
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
