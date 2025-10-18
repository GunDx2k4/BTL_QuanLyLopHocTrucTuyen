using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Models;

public class Submission : Entity
{
    [Required]
    [ForeignKey("Assignment")]
    public Guid AssignmentId { get; set; }
    [JsonIgnore]
    public Assignment? Assignment { get; set; }

    [Required]
    [ForeignKey("Student")]
    public Guid StudentId { get; set; }
    [JsonIgnore]
    public User? Student { get; set; }

    [DataType(DataType.Date)]
    public DateTime SubmittedAt { get; set; }

    public float? Grade { get; set; }

    [Url]
    public string? FileUrl { get; set; }
}
