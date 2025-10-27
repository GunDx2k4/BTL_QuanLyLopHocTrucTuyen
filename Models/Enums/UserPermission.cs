namespace BTL_QuanLyLopHocTrucTuyen.Models.Enums;

[Flags]
public enum UserPermission : ulong
{
    // Guest or no permissions
    None = 0,

    // Tenant-level permissions
    CreateTenant = 1UL << 0, // Create tenant
    EditTenant = 1UL << 1, // Update tenant info
    DeleteTenant = 1UL << 2, // Delete tenant
    ViewTenant = 1UL << 3, // View tenant details
    CreateRole = 1UL << 4, // Create roles
    EditRole = 1UL << 5, // Update roles
    DeleteRole = 1UL << 6, // Delete roles
    ViewRole = 1UL << 7, // View role details
    ViewRoles = 1UL << 8, // View roles in tenant
    CreateUser = 1UL << 9, // Create users
    EditUser = 1UL << 10, // Update user info
    EditUsers = 1UL << 11, // Update multiple users
    DeleteUser = 1UL << 12, // Delete users
    ViewUser = 1UL << 13, // View user details
    ViewUsers = 1UL << 14, // View users in tenant

    // Course-level permissions
    CreateCourse = 1UL << 15, // Create courses
    EditCourse = 1UL << 16, // Update course info
    DeleteCourse = 1UL << 17, // Delete courses
    ViewCourse = 1UL << 18, // View course details
    ViewCourses = 1UL << 19, // View courses in tenant
    EnrollCourses = 1UL << 20, // Enroll in courses
    EnrollStudents = 1UL << 21, // Enroll students to courses
    ManagerInstructors = 1UL << 22, // Manage instructors in courses

    // Lesson-level permissions
    CreateLesson = 1UL << 23, // Create lessons
    EditLesson = 1UL << 24, // Update lesson info
    DeleteLesson = 1UL << 25, // Delete lessons
    ViewLesson = 1UL << 26, // View lesson details
    ViewLessons = 1UL << 27, // View lessons

    // Assignment-level permissions
    CreateAssignment = 1UL << 28, // Create assignments
    EditAssignment = 1UL << 29, // Update assignment info
    DeleteAssignment = 1UL << 30, // Delete assignments
    ViewAssignment = 1UL << 31, // View assignment details
    ViewAssignments = 1UL << 32, // View assignments

    // Submission-level permissions
    CreateSubmission = 1UL << 33, // Create submissions
    EditSubmission = 1UL << 34, // Update submission info
    DeleteSubmission = 1UL << 35, // Delete submissions
    ViewSubmission = 1UL << 36, // View submission details
    ViewSubmissions = 1UL << 37, // View submissions
    GradeSubmissions = 1UL << 38, // Grade submissions

    // Material-level permissions
    CreateMaterial = 1UL << 39, // Create materials
    EditMaterial = 1UL << 40, // Update material info
    DeleteMaterial = 1UL << 41, // Delete materials
    ViewMaterial = 1UL << 42, // View material details
    ViewMaterials = 1UL << 43, // View materials

    // Admin-level permissions
    ViewDashboard = 1UL << 44, // View admin dashboard
    ManageAllTenants = 1UL << 45, // Manage all tenants
    ManageAllUsers = 1UL << 46, // Manage all users
    ManageAllRoles = 1UL << 47, // Manage all roles
    FullAccess = ulong.MaxValue,


    //Role
    BasicUser = ViewUser | EditUser,

    Administrator = ViewDashboard | ManageAllTenants | ManageAllUsers | ManageAllRoles | BasicUser,

    Manager = CreateUser | CreateCourse | EditCourse | DeleteCourse | ViewCourse | ViewCourses |
              EnrollStudents | ManagerInstructors | ViewUsers | EditUsers | BasicUser,
    Instructor = ViewCourse | CreateLesson | EditLesson | DeleteLesson | ViewLesson | ViewLessons |
                 CreateAssignment | EditAssignment | DeleteAssignment | ViewAssignment | ViewAssignments |
                 CreateMaterial | EditMaterial | DeleteMaterial | ViewMaterial | ViewMaterials |
                 ViewSubmission | ViewSubmissions | GradeSubmissions | BasicUser,
    Student = EnrollCourses | ViewCourse | CreateSubmission | EditSubmission | DeleteSubmission |
              ViewSubmission | BasicUser

}
