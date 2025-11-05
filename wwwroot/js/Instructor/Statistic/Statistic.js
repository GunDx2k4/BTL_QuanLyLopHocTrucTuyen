$(document).ready(function () {
    const $search = $("#courseSearch");
    const $dropdown = $("#courseDropdown");
    const $clear = $("#clearCourse");

    // 🔍 Tìm kiếm khóa học
    $search.on("input focus", function () {
        const keyword = $(this).val().toLowerCase().trim();
        $dropdown.children("li").each(function () {
            $(this).toggle($(this).text().toLowerCase().includes(keyword));
        });
        $dropdown.show();
    });

    // ✅ Chọn khóa học
    $dropdown.on("click", "li", function () {
        const name = $(this).text();
        const id = $(this).data("id");
        $search.val(name);
        $dropdown.hide();
        $clear.show();
        $.ajax({
            url: `/Instructor/SelectCourse`,
            type: "POST",
            data: { courseId: id },
            success: function () { location.reload(); },
            error: function () { alert("Không thể chọn khóa học."); }
        });
    });

    // ❌ Xóa khóa học
    $clear.on("click", function () {
        $search.val("");
        $clear.hide();
        $dropdown.show();
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest(".filter-course").length) {
            $dropdown.hide();
        }
    });
});
