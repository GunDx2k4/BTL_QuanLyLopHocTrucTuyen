$(document).ready(function () {

    // ===== Khởi tạo Select2 có hỗ trợ tìm kiếm không dấu =====
    function removeVietnamese(str) {
        return str.normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/đ/g, 'd').replace(/Đ/g, 'D');
    }

    const $input = $("#lessonSearch");
    const $dropdown = $("#lessonDropdown");
    const $hidden = $("#LessonId");
    const $clearBtn = $("#clearLesson");

    // Khi gõ tìm kiếm
    $input.on("input focus", function () {
        const keyword = removeVietnamese($(this).val().toLowerCase().trim());
        let hasResult = false;

        // Nếu chưa nhập gì → hiện toàn bộ
        if (keyword === "") {
            $dropdown.children("li").show();
            hasResult = true;
        } else {
            $dropdown.children("li").each(function () {
                const text = removeVietnamese($(this).text().toLowerCase());
                if (text.includes(keyword)) {
                    $(this).show();
                    hasResult = true;
                } else {
                    $(this).hide();
                }
            });
        }

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

    // Khi nhấn X — xóa lựa chọn
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

    // Ban đầu ẩn nút xóa
    $clearBtn.hide();

    // ===== Sự kiện gửi form bằng AJAX =====
     $("#materialForm").on("submit", function (e) {
        e.preventDefault();
        const form = $(this)[0];
        const $btn = $(this).find("button[type='submit']");

        // Kiểm tra hợp lệ
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
            processData: false,  // Quan trọng để giữ dạng FormData
            contentType: false,  // Quan trọng để ASP.NET nhận đúng file
            success: function (res) {
                if (res.success) {
                    showToast("✅ Thêm tài liệu thành công!");
                    setTimeout(() => (window.location.href = "/Instructor/Material"), 1000);
                } else {
                    const msg = res.errors ? res.errors.join(", ") : "Không rõ lỗi!";
                    showToast("❌ " + msg, true);
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("❌ Lỗi khi thêm tài liệu. Vui lòng thử lại!", true);
            },
            complete: function () {
                $btn.prop("disabled", false).html('<i class="bi bi-floppy2"></i> Lưu');
            }
        });
    });
    // ===== Toast thông báo =====
    function showToast(message, isError = false) {
        const toast = $("<div></div>")
            .addClass("custom-toast")
            .text(message)
            .css({
                backgroundColor: isError ? "#dc3545" : "#28a745"
            })
            .appendTo("body")
            .delay(2000)
            .fadeOut(400, function () { $(this).remove(); });
    }
});
