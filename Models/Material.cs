using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models
{
    public class Material : Entity
    {
        // ===== 🧩 THÔNG TIN CƠ BẢN =====
        [Required(ErrorMessage = "Tên tài liệu là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên tài liệu không được vượt quá 200 ký tự")]
        [Display(Name = "Tên tài liệu")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // ===== 🌐 LINK NGOÀI =====
        [Url(ErrorMessage = "Đường dẫn tài liệu không hợp lệ")]
        [Display(Name = "Liên kết ngoài (Google Drive, PDF, Docs...)")]
        public string? ExternalFileUrl { get; set; }

        // ===== 💾 FILE NỘI BỘ =====
        [MaxLength(255)]
        [Display(Name = "Tên file nội bộ")]
        public string? UploadedFileName { get; set; }

        [Display(Name = "Đường dẫn file nội bộ")]
        public string? UploadedFileUrl { get; set; }

        [NotMapped]
        [Display(Name = "Tệp tải lên (tùy chọn)")]
        public IFormFile? UploadFile { get; set; }

        // ===== 🔗 LIÊN KẾT BÀI HỌC =====
        [Required(ErrorMessage = "Bài học là bắt buộc")]
        [ForeignKey("Lesson")]
        [Display(Name = "Bài học")]
        public Guid LessonId { get; set; }

        [JsonIgnore]
        public Lesson? Lesson { get; set; }

        // ===== 👤 NGƯỜI TẢI LÊN =====
        [ForeignKey("Uploader")]
        [Display(Name = "Người tải lên")]
        public Guid? UploadedBy { get; set; }

        [JsonIgnore]
        public User? Uploader { get; set; }

        // ===== ⏰ THỜI GIAN =====
        [Display(Name = "Thời gian tải lên")]
        [DataType(DataType.DateTime)]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // ===== 🌍 TRẠNG THÁI CÔNG KHAI =====
        [Display(Name = "Công khai cho sinh viên")]
        public bool IsPublic { get; set; } = false;
        public string FileUrl { get; set; }

    }
}
