using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class User : Entity
{
    [Required]
    [MaxLength(100)]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [ForeignKey("Tenant")]
    public Guid? TenantId { get; set; }          
    [JsonIgnore]
    public Tenant? Tenant { get; set; }

    [ForeignKey("Role")]
    public Guid? RoleId { get; set; }
    [JsonIgnore]
    public Role? Role { get; set; }
    
    [JsonIgnore]
    public ICollection<Course> InstructedCourses { get; set; } = new List<Course>();
    [JsonIgnore]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    [JsonIgnore]
    public ICollection<Material> UploadedMaterials { get; set; } = new List<Material>();
    [JsonIgnore]
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
