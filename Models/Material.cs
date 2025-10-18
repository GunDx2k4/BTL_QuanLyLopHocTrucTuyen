using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Material : Entity
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Url]
    public string? FileUrl { get; set; }

    [Required]
    [ForeignKey("Lesson")]
    public Guid LessonId { get; set; }
    [JsonIgnore]
    public Lesson? Lesson { get; set; }

    [ForeignKey("Uploader")]
    public Guid? UploadedBy { get; set; }
    [JsonIgnore]
    public User? Uploader { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
