using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Material : Entity
{
    [Required(ErrorMessage = "Tên tài liệu là bắt buộc")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;


    [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    [DataType(DataType.Url, ErrorMessage = "Đường dẫn tài liệu không hợp lệ")]
    [Display(Name = "Liên kết tài liệu")]
    public string? FileUrl { get; set; }

    [Required]
    [ForeignKey("Lesson")]
    [Display(Name = "Bài học")]
    public Guid LessonId { get; set; }
    [JsonIgnore]
    public Lesson? Lesson { get; set; }

    [ForeignKey("Uploader")]
    [Display(Name = "Người tải lên")]
    public Guid? UploadedBy { get; set; }
    [JsonIgnore]
    public User? Uploader { get; set; }

    [Display(Name = "Thời gian tải lên")]
    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Vui lòng chọn thời gian tải lên")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
