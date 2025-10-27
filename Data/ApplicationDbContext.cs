using System;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Data;

public abstract class ApplicationDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected ApplicationDbContext(DbContextOptions options) : base(options)
    {  
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // === CORE RELATIONSHIPS ===

        // User -> Role (N:1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull); // Nếu xóa Role, user không mất mà RoleId = null

        // Tenant -> Owner (1:1)
        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.Owner)
            .WithOne()
            .HasForeignKey<Tenant>(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // Không cho xóa Owner nếu còn Tenant

        // Tenant -> Users (1:N)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Tenant sẽ xóa luôn tất cả Users thuộc Tenant

        // Tenant -> Roles (1:N)
        modelBuilder.Entity<Role>()
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Roles)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Tenant xóa luôn Roles

        // Tenant -> Courses (1:N)
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Tenant)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Tenant xóa luôn Courses

        // === COURSE RELATIONSHIPS ===

        // Course -> Instructor (N:1)
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.InstructedCourses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.SetNull); // Instructor bị xóa => khóa học vẫn giữ

        // Course -> Lessons (1:N)
        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Course xóa Lessons

        // === LESSON RELATIONSHIPS ===

        // Lesson -> Assignments (1:N)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Lesson)
            .WithMany(l => l.Assignments)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Lesson xóa Assignment

        // Lesson -> Materials (1:N)
        modelBuilder.Entity<Material>()
            .HasOne(m => m.Lesson)
            .WithMany(l => l.Materials)
            .HasForeignKey(m => m.LessonId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Lesson xóa Material

        // Material -> Uploader (N:1)
        modelBuilder.Entity<Material>()
            .HasOne(m => m.Uploader)
            .WithMany(u => u.UploadedMaterials)
            .HasForeignKey(m => m.UploadedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // === ENROLLMENT & SUBMISSION ===

        // Enrollment -> User (N:1)
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa User xóa Enrollment

        // Enrollment -> Course (N:1)
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Course xóa Enrollment

        // Assignment -> Submissions (1:N)
        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Assignment xóa Submission

        // Submission -> Student (N:1)
        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Student xóa Submission
    }

}
