$(document).ready(function () {
    const $courseSearch = $("#courseSearch");
    const $courseDropdown = $("#courseDropdown");
    const $clearCourse = $("#clearCourse");

    const $assignmentSearch = $("#assignmentSearch");
    const $assignmentDropdown = $("#assignmentDropdown");
    const $clearAssignment = $("#clearAssignment");

    const $tableBody = $("#gradeTableBody");

    let currentCourseId = $(".filter-course").data("current-course") || "";
    let currentAssignmentId = $(".filter-assignment").data("current-assignment") || "";

    // ======================== COURSE ========================
    if (currentCourseId) {
        const item = $courseDropdown.find(`[data-id='${currentCourseId}']`);
        if (item.length) {
            $courseSearch.val(item.text().trim());
            item.addClass("active");
            $clearCourse.show();
        }
    }

    $courseSearch.on("input focus", function () {
        const keyword = $(this).val().toLowerCase().trim();
        $courseDropdown.children("li").each(function () {
            $(this).toggle($(this).text().toLowerCase().includes(keyword));
        });
        $courseDropdown.show();
    });

    $courseDropdown.on("click", "li", function () {
        const id = $(this).data("id");
        const name = $(this).text().trim();
        currentCourseId = id;
        $courseSearch.val(name);
        $courseDropdown.hide();
        $clearCourse.show();
        $courseDropdown.find("li").removeClass("active");
        $(this).addClass("active");
        loadAssignments(id);
    });

    $clearCourse.on("mousedown", function (e) {
        e.preventDefault();
        e.stopPropagation();
        setTimeout(() => {
            $courseSearch.val("");
            $clearCourse.hide();
            currentCourseId = "";
            $courseDropdown.find("li").removeClass("active");
            $courseDropdown.show();
        }, 60);
    });

    // ======================== ASSIGNMENT ========================
    if (currentAssignmentId) {
        const item = $assignmentDropdown.find(`[data-id='${currentAssignmentId}']`);
        if (item.length) {
            $assignmentSearch.val(item.text().trim());
            item.addClass("active");
            $clearAssignment.show();
        }
    }

    $assignmentSearch.on("input focus", function () {
        const keyword = $(this).val().toLowerCase().trim();
        $assignmentDropdown.children("li").each(function () {
            $(this).toggle($(this).text().toLowerCase().includes(keyword));
        });
        $assignmentDropdown.show();
    });

    $assignmentDropdown.on("click", "li", function () {
        const id = $(this).data("id");
        const name = $(this).text().trim();
        currentAssignmentId = id;
        $assignmentSearch.val(name);
        $assignmentDropdown.hide();
        $clearAssignment.show();
        $assignmentDropdown.find("li").removeClass("active");
        $(this).addClass("active");
        loadGrades(currentCourseId, id);
    });

    $clearAssignment.on("mousedown", function (e) {
        e.preventDefault();
        e.stopPropagation();
        setTimeout(() => {
            $assignmentSearch.val("");
            $clearAssignment.hide();
            currentAssignmentId = "";
            $assignmentDropdown.find("li").removeClass("active");
            $assignmentDropdown.show();
        }, 60);
    });

    // Ẩn dropdown khi click ra ngoài
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".filter-course").length) $courseDropdown.hide();
        if (!$(e.target).closest(".filter-assignment").length) $assignmentDropdown.hide();
    });

    // ======================== AJAX ========================
    function loadAssignments(courseId) {
        $.ajax({
            url: `/Instructor/Grade`,
            type: "GET",
            data: { courseId },
            success: function (html) {
                const page = $(html);
                const newList = page.find("#assignmentDropdown").html();
                $("#assignmentDropdown").html(newList);
                $("#assignmentSearch").val("");
                $("#clearAssignment").hide();
            },
            error: () => alert("⚠️ Lỗi khi tải danh sách bài tập!"),
        });
    }

    function loadGrades(courseId, assignmentId) {
        $tableBody.html(`
            <tr>
                <td colspan="8" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status"></div>
                    <div class="mt-2 text-muted">Đang tải dữ liệu điểm...</div>
                </td>
            </tr>
        `);
        $.ajax({
            url: `/Instructor/Grade`,
            type: "GET",
            data: { courseId, assignmentId },
            success: function (html) {
                const page = $(html);
                const newTable = page.find("#gradeTableBody").html() || "";
                $tableBody.hide().html(newTable).fadeIn(200);
            },
            error: () => {
                $tableBody.html(`
                    <tr>
                        <td colspan="8" class="text-center text-danger py-4">
                            ⚠️ Lỗi khi tải dữ liệu điểm.
                        </td>
                    </tr>
                `);
            },
        });
    }
});
