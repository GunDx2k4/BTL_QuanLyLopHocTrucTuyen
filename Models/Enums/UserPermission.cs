namespace BTL_QuanLyLopHocTrucTuyen.Models.Enums;

[Flags]
public enum UserPermission : uint
{
    // Guest or no permissions
    None = 0,

    // Tenant-level permissions
    ManagerTenant = 1 << 0, // Manager name, plan, etc.
    ManagerRoles = 1 << 1, // Create, update, delete roles
    ManagerUsers = 1 << 2, // Create, update, delete users

    // Course-level permissions
    CreateCourse = 1 << 3, // Create courses
    EditCourse = 1 << 4, // Update course info
    DeleteCourse = 1 << 5, // Delete courses
    ViewCourse = 1 << 6, // View course details
    EnrollStudents = 1 << 7, // Enroll students to courses
    ManagerInstructors = 1 << 8, // Manage instructors in courses

    // Lesson-level permissions
    CreateLesson = 1 << 9, // Create lessons
    EditLesson = 1 << 10, // Update lesson info
    DeleteLesson = 1 << 11, // Delete lessons
    ViewLesson = 1 << 12, // View lesson details

    // Assignment-level permissions
    CreateAssignment = 1 << 13, // Create assignments
    EditAssignment = 1 << 14, // Update assignment info
    DeleteAssignment = 1 << 15, // Delete assignments
    ViewAssignment = 1 << 16, // View assignment details

    // Submission-level permissions
    CreateSubmission = 1 << 16, // Create submissions
    EditSubmission = 1 << 17, // Update submission info
    DeleteSubmission = 1 << 18, // Delete submissions
    ViewSubmission = 1 << 19, // View submission details
    ViewSubmissions = 1 << 20, // View submissions
    GradeSubmissions = 1 << 21, // Grade submissions

    // Material-level permissions
    CreateMaterial = 1 << 22, // Create materials
    EditMaterial = 1 << 23, // Update material info
    DeleteMaterial = 1 << 24, // Delete materials
    ViewMaterial = 1 << 25, // View material details

    // Admin-level permissions
    Admin = 1 << 30 // All permissions
}
