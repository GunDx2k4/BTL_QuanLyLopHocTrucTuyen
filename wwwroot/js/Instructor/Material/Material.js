$(document).ready(function () {

    /* =====================================================
       🔍 TÌM KIẾM TÀI LIỆU (hỗ trợ tiếng Việt có dấu)
    ===================================================== */
     $(".search-input, .filter-select").on("input change", function () {
        const keyword = removeVietnameseTones($(".search-input").val().toLowerCase().trim());
        const selectedLesson = $(".filter-select").val(); // ID bài học được chọn

        $(".lesson-section").each(function () {
            const groupTitle = removeVietnameseTones($(this).find(".lesson-title").text().toLowerCase());
            const lessonId = $(this).data("lesson-id")?.toString(); // lấy đúng LessonId

            const matchKeyword = groupTitle.includes(keyword);
            const matchLesson = !selectedLesson || selectedLesson === lessonId;

            $(this).toggle(matchKeyword && matchLesson);
        });
    });

    // ===== Hàm loại bỏ dấu tiếng Việt =====
    function removeVietnameseTones(str) {
        return str
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/đ/g, "d")
            .replace(/Đ/g, "D");
    }

    /* =====================================================
       🗑️ XÓA TÀI LIỆU (Material)
    ===================================================== */
    $(".btn-delete").on("click", function () {
        const materialId = $(this).data("id");
        const title = $(this).closest(".lesson-card").find("h6").text().trim();

        if (!materialId) {
            showToast("Không tìm thấy ID tài liệu để xóa!", true);
            return;
        }

        if (confirm(`Bạn có chắc muốn xóa tài liệu "${title}" không?`)) {
            $.ajax({
                url: `/Instructor/DeleteMaterial?id=${materialId}`, // 🔹 Đúng route controller
                type: "DELETE",
                success: function (response) {
                    if (response.success) {
                        showToast(`🗑️ Đã xóa "${title}" thành công!`);
                        $(`.btn-delete[data-id='${materialId}']`)
                            .closest(".lesson-card")
                            .slideUp(300, function () { $(this).remove(); });
                    } else {
                        showToast("❌ Xóa thất bại: " + (response.message || "Lỗi không xác định!"), true);
                    }
                },
                error: function (xhr) {
                    console.error(xhr.responseText);
                    showToast("❌ Có lỗi khi xóa tài liệu!", true);
                }
            });
        }
    });

    /* =====================================================
       🔔 HÀM HIỂN THỊ THÔNG BÁO NHỎ (TOAST)
    ===================================================== */
    function showToast(message, isError = false) {
        const toast = $("<div></div>")
            .text(message)
            .addClass("custom-toast")
            .css({
                position: "fixed",
                bottom: "20px",
                right: "20px",
                backgroundColor: isError ? "#dc3545" : "#198754",
                color: "white",
                padding: "12px 20px",
                borderRadius: "6px",
                boxShadow: "0 2px 6px rgba(0,0,0,0.3)",
                zIndex: 9999,
                opacity: 0
            })
            .appendTo("body")
            .animate({ opacity: 1 }, 300)
            .delay(2000)
            .fadeOut(500, function () { $(this).remove(); });
    }

});
