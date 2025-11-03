$(document).ready(function () {
    document.querySelectorAll(".lesson-header").forEach(header => {
            header.addEventListener("click", () => {
                header.classList.toggle("active");
            });
        });

    /* =====================================================
       🔍 TÌM KIẾM BÀI TẬP
    ===================================================== */
    $(".search-input").on("input", function () {
        const keyword = removeVietnameseTones($(this).val().toLowerCase().trim());

        if (keyword === "") {
            $(".lesson-group").show();
            $(".material-card").show();
            return;
        }

        $(".lesson-group").each(function () {
            let matchFound = false;
            const lessonTitle = removeVietnameseTones($(this).find(".lesson-title").text().toLowerCase());

            $(this).find(".material-card").each(function () {
                const title = removeVietnameseTones($(this).find(".material-title").text().toLowerCase());

                const isMatch =
                    title.includes(keyword) ||
                    lessonTitle.includes(keyword);

                $(this).toggle(isMatch);
                if (isMatch) matchFound = true;
            });

            $(this).toggle(matchFound);

            // ✅ Nếu tìm thấy kết quả trong lesson, tự mở ra
            if (matchFound) {
                $(this).find(".lesson-material-list").addClass("show").collapse("show");
                $(this).find(".lesson-header").addClass("active");
            }
        });
    });

    /* =====================================================
    🔠 HÀM LOẠI BỎ DẤU TIẾNG VIỆT
    ===================================================== */
    function removeVietnameseTones(str) {
        if (!str) return "";
        return str
            .normalize("NFD")                     // tách dấu ra khỏi ký tự
            .replace(/[\u0300-\u036f]/g, "")      // xóa các dấu thanh
            .replace(/đ/g, "d").replace(/Đ/g, "D")// thay đ → d
            .replace(/[^a-zA-Z0-9\s]/g, "");      // loại bỏ ký tự đặc biệt
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
            url: `/Instructor/TogglePublicMaterial?id=${id}`,
            type: "POST",
            success: function (res) {
                if (!res.success) {
                    showToast("❌ " + (res.message || "Không thể cập nhật trạng thái!"), true);
                    return;
                }

                const isPublic = res.isPublic;
                const card = btn.closest(".material-card");

                // 1) đổi nút
                btn.toggleClass("btn-success btn-outline-secondary");
                btn.find("i").toggleClass("bi-eye bi-eye-slash");

                // 2) đổi phần trạng thái ở meta
                const statusWrap = card.find(".material-status");
                const icon = statusWrap.find("i");
                const text = statusWrap.find(".status-text");

                if (isPublic) {
                    icon.removeClass("bi-eye-slash text-secondary")
                        .addClass("bi-eye text-success");
                    text.removeClass("text-secondary")
                        .addClass("text-success")
                        .text("Công khai");
                } else {
                    icon.removeClass("bi-eye text-success")
                        .addClass("bi-eye-slash text-secondary");
                    text.removeClass("text-success")
                        .addClass("text-secondary")
                        .text("Riêng tư");
                }

                showToast(isPublic ? "👁️ Tài liệu đã được công khai!" : "🙈 Tài liệu đã được ẩn!", false);
            },
            error: function () {
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
