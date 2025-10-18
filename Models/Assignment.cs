using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Assignment : Entity
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    [Required]
    [ForeignKey("Lesson")]
    public Guid LessonId { get; set; }
    [JsonIgnore]
    public Lesson? Lesson { get; set; }
    
    [JsonIgnore]
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

}
