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


    $("#assignmentForm").on("submit", function (e) {
        e.preventDefault();
        const form = $(this);

        if (!form.valid()) {
            showToast("⚠️ Vui lòng nhập đầy đủ thông tin!", true);
            return;
        }

        // 🟢 Dùng FormData để gửi cả file
        const formData = new FormData(this);

        $.ajax({
            url: form.attr("action"),
            type: "POST",
            data: formData,
            processData: false,   // không chuyển formData thành query string
            contentType: false,   // để trình duyệt tự thêm multipart/form-data
            success: function () {
                showToast("✅ Thêm bài tập thành công!");
                setTimeout(() => window.location.href = "/Instructor/Assignment", 1000);
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("❌ Lỗi khi thêm bài tập. Vui lòng thử lại!", true);
            }
        });
    });

    // Hàm toast thông báo
    function showToast(message, isError = false) {
        const toast = $("<div></div>")
            .text(message)
            .addClass("custom-toast")
            .css({
                position: "fixed",
                bottom: "20px",
                right: "20px",
                backgroundColor: isError ? "#dc3545" : "#28a745",
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
            .fadeOut(400, function () { $(this).remove(); });
    }
});
