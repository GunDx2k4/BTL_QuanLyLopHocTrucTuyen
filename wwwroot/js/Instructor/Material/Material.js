$(document).ready(function () {

    /* =====================================================
       🔍 TÌM KIẾM TÀI LIỆU (hỗ trợ tiếng Việt có dấu)
    ===================================================== */
    $(".search-input, .filter-select").on("input change", function () {
        const keyword = removeVietnameseTones($(".search-input").val().toLowerCase().trim());
        const selectedLesson = $(".filter-select").val(); // ID bài học được chọn

        $(".lesson-section").each(function () {
            const lessonId = $(this).data("lesson-id")?.toString();
            const lessonTitle = removeVietnameseTones($(this).find(".lesson-title").text().toLowerCase());

            let hasMatch = false;

            $(this).find(".material-card").each(function () {
                const materialTitle = removeVietnameseTones($(this).find("h6").text().toLowerCase());

                const matchKeyword =
                    !keyword ||
                    lessonTitle.includes(keyword) ||
                    materialTitle.includes(keyword);

                const matchLesson =
                    !selectedLesson || selectedLesson === lessonId;

                const isVisible = matchKeyword && matchLesson;

                $(this).toggle(isVisible);
                if (isVisible) hasMatch = true;
            });

            $(this).toggle(hasMatch);
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
   🌍 CÔNG KHAI / ẨN TÀI LIỆU (DÙNG EVENT DELEGATION)
    ===================================================== */
    $(document).on("click", ".btn-toggle", function () {
        const btn = $(this);
        const id = btn.data("id");

        if (!id) {
            showToast("❌ Không tìm thấy ID tài liệu!", true);
            return;
        }

        $.ajax({
            url: `/Instructor/TogglePublicMaterial?id=${id}`, // Gửi id qua query string
            type: "POST",
            success: function (res) {
                console.log("TogglePublic response:", res);
                if (res.success) {
                    const isPublic = res.isPublic;
                    const card = btn.closest(".material-card");

                    // Đổi icon & màu nút
                    btn.toggleClass("btn-success btn-outline-secondary");
                    btn.find("i").toggleClass("bi-eye bi-eye-slash");

                    // Cập nhật text trạng thái
                    card.find(".meta .status-text").remove();
                    const statusHtml = `<span class="status-text ms-1 text-${isPublic ? "success" : "secondary"}">
                        ${isPublic ? "Công khai" : "Riêng tư"}
                    </span>`;
                    card.find(".meta").append(statusHtml);

                    // Hiển thị thông báo
                    showToast(
                        isPublic
                            ? "👁️ Tài liệu đã được công khai!"
                            : "🙈 Tài liệu đã được ẩn đi!"
                    );
                } else {
                    showToast("❌ " + (res.message || "Không thể cập nhật trạng thái!"), true);
                }
            },
            error: function (xhr) {
                console.error("TogglePublic error:", xhr.responseText);
                showToast("⚠️ Lỗi khi đổi trạng thái công khai!", true);
            }
        });
    });



    /* =====================================================
       🗑️ XÓA TÀI LIỆU
    ===================================================== */
    $(document).on("click", ".btn-delete", function () {
        const materialId = $(this).data("id");
        const title = $(this).closest(".material-card").find("h6").text().trim();

        if (!materialId) {
            showToast("Không tìm thấy ID tài liệu để xóa!", true);
            return;
        }

        if (confirm(`Bạn có chắc muốn xóa tài liệu "${title}" không?`)) {
            $.ajax({
                url: `/Instructor/DeleteMaterial?id=${materialId}`,
                type: "DELETE",
                success: function (response) {
                    if (response.success) {
                        showToast(`🗑️ Đã xóa "${title}" thành công!`);
                        const card = $(`.btn-delete[data-id='${materialId}']`).closest(".material-card");

                        card.fadeOut(300, function () {
                            $(this).remove();

                            // Nếu section không còn tài liệu → ẩn nhóm
                            $(".lesson-section").each(function () {
                                if ($(this).find(".material-card").length === 0) {
                                    $(this).slideUp(300);
                                }
                            });
                        });
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
       🔔 HIỂN THỊ TOAST THÔNG BÁO
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
