using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Core.Controllers
{
    
    [UserPermissionAuthorize(UserPermission.Instructor)]
    public class BaseInstructorController : Controller
    {
        // ✅ Lấy ID khóa học đang chọn
        protected Guid? GetCurrentCourseId()
        {
            var claim = User.FindFirst("CurrentCourseId");
            if (claim == null) return null;

            return Guid.TryParse(claim.Value, out var id) ? id : null;
        }

        // ✅ Lấy tên khóa học đang chọn (nếu có)
        protected string? GetCurrentCourseName()
        {
            return User.FindFirst("CurrentCourseName")?.Value;
        }

        // ✅ Kiểm tra đã chọn khóa học chưa (áp dụng cho cả View và AJAX)
        protected IActionResult? EnsureCourseSelected()
        {
            var courseId = GetCurrentCourseId();

            // ❌ Nếu chưa chọn
            if (courseId == null || courseId == Guid.Empty)
            {
                // 🧩 Nếu là AJAX request → trả JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        requireCourse = true,
                        message = "Vui lòng chọn khóa học trước khi thực hiện thao tác này!"
                    });
                }

                // 🧩 Nếu là request thường → redirect kèm TempData cảnh báo
                TempData["CourseWarning"] = "⚠️ Vui lòng chọn khóa học trước khi xem nội dung.";
                return Redirect("/Instructor/Course?requireCourse=true");
            }

            return null; // OK
        }
    }
}
