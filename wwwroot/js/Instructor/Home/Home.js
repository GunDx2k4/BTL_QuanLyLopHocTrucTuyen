$(document).ready(function () {
    $(".event-item, .lesson-item").on("click", function (e) {
        e.preventDefault();

        const courseId = $(this).data("courseid");
        const type = $(this).data("type");

        if (!courseId) {
            alert("⚠️ Không xác định được khóa học!");
            return;
        }

        // Chọn khóa học trước
        $.ajax({
            url: `/Instructor/SelectCourse?courseId=${courseId}`,
            type: "POST",
            success: function () {
                if (type === "lesson")
                    window.location.href = "/Instructor/Lesson";
                else if (type === "assignment")
                    window.location.href = "/Instructor/Assignment";
            },
            error: function () {
                alert("❌ Không thể mở khóa học này!");
            }
        });
    });
});
