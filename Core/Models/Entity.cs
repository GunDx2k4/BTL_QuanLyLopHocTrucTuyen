using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLyLopHocTrucTuyen.Core.Models;

public class Entity : IEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}
