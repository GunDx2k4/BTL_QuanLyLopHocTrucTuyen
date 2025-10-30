$(document).ready(function () {

    /* =====================================================
       🔍 TÌM KIẾM BÀI TẬP
    ===================================================== */
    $(".search-input").on("input", function () {
        const keyword = $(this).val().toLowerCase().trim();
        $(".assignment-card").each(function () {
            const title = $(this).find(".assignment-title").text().toLowerCase();
            const desc = $(this).find(".assignment-desc").text().toLowerCase();
            $(this).toggle(title.includes(keyword) || desc.includes(keyword));
        });
    });


    /* =====================================================
       🧭 SẮP XẾP DANH SÁCH BÀI TẬP
    ===================================================== */
    $(".sort-select").on("change", function () {
        const sortType = $(this).val();
        const assignments = $(".assignment-card").get();

        assignments.sort((a, b) => {
            const startA = parseDate($(a).find(".meta span:nth-child(1)").text());
            const startB = parseDate($(b).find(".meta span:nth-child(1)").text());
            const dueA = parseDate($(a).find(".meta span:nth-child(2)").text());
            const dueB = parseDate($(b).find(".meta span:nth-child(2)").text());
            const scoreA = parseInt($(a).find(".meta strong").text()) || 0;
            const scoreB = parseInt($(b).find(".meta strong").text()) || 0;
            const typeA = $(a).find(".meta span:contains('Loại')").text().toLowerCase();
            const typeB = $(b).find(".meta span:contains('Loại')").text().toLowerCase();

            switch (sortType) {
                case "oldest": return startA - startB;
                case "deadline": return dueA - dueB;
                case "type": return typeA.localeCompare(typeB);
                case "score": return scoreB - scoreA;
                default: return startB - startA; // newest
            }
        });

        $(".assignment-list").empty().append(assignments);
    });

    // 🔹 Hàm chuyển text ngày về kiểu Date
    function parseDate(text) {
        const cleaned = text.replace("Bắt đầu:", "").replace("Hạn nộp:", "").trim();
        const parts = cleaned.split(/[\s/:]/);
        if (parts.length >= 5) {
            const [day, month, year, hour, minute] = parts.map(p => parseInt(p, 10));
            return new Date(year, month - 1, day, hour || 0, minute || 0);
        }
        return new Date(cleaned) || new Date(0);
    }


    /* =====================================================
       🗑️ XÓA BÀI TẬP (DÙNG EVENT DELEGATION)
    ===================================================== */
    $(document).on("click", ".btn-delete", function () {
        const id = $(this).data("id");
        const card = $(this).closest(".assignment-card");
        const title = card.find(".assignment-title").text().trim();

        if (!id) return showToast("Không tìm thấy ID bài tập để xóa!", true);
        if (!confirm(`Bạn có chắc muốn xóa bài tập "${title}" không?`)) return;

        $.ajax({
            url: `/Instructor/DeleteAssignment?id=${id}`,
            type: "DELETE",
            success: function (res) {
                if (res.success) {
                    card.fadeOut(300, () => card.remove());
                    showToast(`🗑️ Đã xóa "${title}" thành công!`);
                } else showToast("❌ Xóa thất bại: " + (res.message || "Lỗi không xác định!"), true);
            },
            error: () => showToast("⚠️ Có lỗi khi xóa bài tập!", true)
        });
    });


    /* =====================================================
       🌍 CÔNG KHAI / ẨN BÀI TẬP (DÙNG EVENT DELEGATION)
    ===================================================== */
    $(document).on("click", ".btn-public", function () {
        const id = $(this).data("id");
        const btn = $(this);
        const card = btn.closest(".assignment-card");

        $.ajax({
            url: `/Instructor/TogglePublicAssignment?id=${id}`,
            type: "POST",
            success: function (res) {
                if (res.success) {
                    const badge = card.find(".assignment-status:first span");
                    if (res.isPublic) {
                        badge.removeClass("bg-secondary").addClass("bg-success")
                             .html('<i class="bi bi-globe"></i> Công khai');
                        btn.find("i").removeClass("bi-globe2").addClass("bi-lock");
                        showToast("🌍 Bài tập đã công khai!");
                    } else {
                        badge.removeClass("bg-success").addClass("bg-secondary")
                             .html('<i class="bi bi-lock"></i> Không công khai');
                        btn.find("i").removeClass("bi-lock").addClass("bi-globe2");
                        showToast("🔒 Bài tập đã ẩn!");
                    }
                } else showToast(res.message || "Không thể cập nhật trạng thái!", true);
            },
            error: () => showToast("❌ Lỗi khi cập nhật trạng thái công khai!", true)
        });
    });


    /* =====================================================
       🔔 THÔNG BÁO NHỎ (TOAST)
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
