$(document).ready(function () {

    /* =====================================================
       🔎 TÌM KIẾM BÀI HỌC KHÔNG DẤU (Lesson Search)
    ===================================================== */
    function removeVietnamese(str) {
        return str
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/đ/g, "d")
            .replace(/Đ/g, "D");
    }

    const $input = $("#lessonSearch");
    const $dropdown = $("#lessonDropdown");
    const $hidden = $("#LessonId");
    const $clearBtn = $("#clearLesson");

    // Khi gõ tìm kiếm
    $input.on("input focus", function () {
        const keyword = removeVietnamese($(this).val().toLowerCase().trim());
        let hasResult = false;

        $dropdown.children("li").each(function () {
            const text = removeVietnamese($(this).text().toLowerCase());
            const visible = text.includes(keyword);
            $(this).toggle(visible);
            if (visible) hasResult = true;
        });

        $dropdown.toggle(hasResult);
    });

    // Khi chọn 1 bài học
    $dropdown.on("click", "li", function () {
        const title = $(this).text();
        const id = $(this).data("id");
        $input.val(title);
        $hidden.val(id);
        $dropdown.hide();
        $clearBtn.show();
    });

    // Khi nhấn X để xóa lựa chọn
    $clearBtn.on("click", function () {
        $input.val("");
        $hidden.val("");
        $clearBtn.hide();
        $input.focus();
        $dropdown.show();
    });

    // Ẩn dropdown khi click ra ngoài
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".lesson-search-wrapper").length) {
            $dropdown.hide();
        }
    });

    // Ban đầu ẩn nút xóa nếu chưa chọn
    if (!$hidden.val()) $clearBtn.hide();


    /* =====================================================
       💾 CẬP NHẬT TÀI LIỆU BẰNG AJAX
    ===================================================== */
    $("#materialForm").on("submit", function (e) {
        e.preventDefault();
        const form = $(this)[0];
        const $btn = $(this).find("button[type='submit']");

        if (!$(form).valid()) {
            showToast("⚠️ Vui lòng nhập đầy đủ thông tin!", true);
            return;
        }

        const formData = new FormData(form);
        $btn.prop("disabled", true).text("⏳ Đang lưu...");

        $.ajax({
            url: $(form).attr("action"),
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.success) {
                    showToast("✅ Cập nhật tài liệu thành công!");
                    setTimeout(() => (window.location.href = "/Instructor/Material"), 1000);
                } else {
                    const msg = res.errors ? res.errors.join(", ") : "Lỗi không xác định!";
                    showToast("❌ " + msg, true);
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("❌ Lỗi khi cập nhật tài liệu. Vui lòng thử lại!", true);
            },
            complete: function () {
                $btn.prop("disabled", false).html('<i class="bi bi-floppy2"></i> Lưu');
            }
        });
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
            .fadeOut(500, function () {
                $(this).remove();
            });
    }
});
