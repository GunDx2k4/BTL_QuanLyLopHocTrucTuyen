using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Role : Entity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Description { get; set; }

    public UserPermission Permissions { get; set; } = UserPermission.None;

    [Required]
    [ForeignKey("Tenant")]
    public Guid TenantId { get; set; }
    [JsonIgnore]
    public Tenant? Tenant { get; set; }

    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}
