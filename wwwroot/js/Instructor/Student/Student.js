$(document).ready(function () {
    const $search = $("#courseSearch");
    const $dropdown = $("#courseDropdown");
    const $clear = $("#clearCourse");
    const $tableBody = $("#studentTableBody");

    // ✅ cập nhật thống kê nhanh
    function updateOverview(html) {
        const newStats = $(html);

        $("#totalStudents").text(newStats.find("#totalStudents").text());
        $("#activeCount").text(newStats.find("#activeCount").text());
        $("#quitCount").text(newStats.find("#quitCount").text());
    }

    // 🔍 Gõ tìm kiếm...
    $search.on("input focus", function () {
        const keyword = $(this).val().toLowerCase().trim();
        let hasResult = false;

        $dropdown.children("li").each(function () {
            const text = $(this).text().toLowerCase();
            const match = text.includes(keyword);
            $(this).toggle(match);
            if (match) hasResult = true;
        });

        if (keyword === "") {
            $dropdown.children("li").show();
            hasResult = true;
        }

        $dropdown.toggle(hasResult);
    });

    // ✅ Khi chọn khóa học — AJAX
    $dropdown.on("click", "li", function () {
        const id = $(this).data("id");
        const name = $(this).text().trim();

        $search.val(name);
        $dropdown.hide();
        $clear.show();

        $tableBody.html(`
            <tr>
                <td colspan="8" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status"></div>
                    <div class="mt-2 text-muted">Đang tải dữ liệu...</div>
                </td>
            </tr>
        `);

        $.ajax({
            url: `/Instructor/Student`,
            type: "GET",
            data: { courseId: id },
            success: function (html) {
                const page = $(html);
                $tableBody.html(page.find("#studentTableBody").html());
                updateOverview(page); // 🔹 cập nhật thống kê
                $dropdown.find("li").removeClass("active");
                $dropdown.find(`[data-id='${id}']`).addClass("active");
            },
            error: function () {
                $tableBody.html(`
                    <tr>
                        <td colspan="8" class="text-center text-danger py-4">
                            ⚠️ Lỗi khi tải danh sách học viên.
                        </td>
                    </tr>
                `);
            }
        });
    });

    // ❌ Nút xóa
    $clear.on("mousedown", function (e) {
        e.preventDefault();
        $search.val("");
        $clear.hide();
        $dropdown.show();

        // $.ajax({
        //     url: `/Instructor/Student`,
        //     type: "GET",
        //     success: function (html) {
        //         const page = $(html);
        //         $tableBody.html(page.find("#studentTableBody").html());
        //         updateOverview(page); // 🔹 cập nhật thống kê
        //         $dropdown.find("li").removeClass("active");
        //     }
        // });
    });
    // Ẩn khi click ra ngoài
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".filter-course").length) {
            $dropdown.hide();
        }
    });

    // 🔄 Lọc theo trạng thái — AJAX
    $("#statusFilter").on("change", function () {
        const status = $(this).val();
        const courseId = $("#courseDropdown .active").data("id") || "";

        $tableBody.html(`
            <tr>
                <td colspan="8" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status"></div>
                    <div class="mt-2 text-muted">Đang tải dữ liệu...</div>
                </td>
            </tr>
        `);

        $.ajax({
            url: `/Instructor/Student`,
            type: "GET",
            data: { courseId: courseId, status: status },
            success: function (html) {
                const page = $(html);
                $tableBody.html(page.find("#studentTableBody").html());
                updateOverview(page); // 🔹 cập nhật thống kê
            },
            error: function () {
                $tableBody.html(`
                    <tr>
                        <td colspan="8" class="text-center text-danger py-4">
                            ⚠️ Lỗi khi tải danh sách học viên.
                        </td>
                    </tr>
                `);
            }
        });
    });
});
