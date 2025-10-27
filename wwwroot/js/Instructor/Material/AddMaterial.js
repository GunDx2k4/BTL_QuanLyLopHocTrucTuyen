$(document).ready(function () {

    // Bắt sự kiện gửi form
    $("#materialForm").on("submit", function (e) {
        e.preventDefault();
        if (!$(this).valid()) {
        showToast("⚠️ Vui lòng nhập đầy đủ thông tin trước khi lưu.", true);
        return;
    }

        const form = $(this);
        const url = form.attr("action");
        const data = form.serialize();

        $.ajax({
            url: url,
            type: "POST",
            data: data,
            success: function (response) {
                // Hiển thị thông báo hoặc redirect
                showToast("✅ Thêm tài liệu thành công!");
                window.location.href = "/Instructor/Material";
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("❌ Lỗi khi thêm tài liệu. Vui lòng thử lại!", true);
            }
        });
    });
    function removeVietnamese(str) {
        return str.normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/đ/g, 'd').replace(/Đ/g, 'D');
    }

    $(".select-lesson").select2({
        placeholder: "Bắt đầu nhập để tìm kiếm...",
        allowClear: true,
        width: "100%",
        dropdownParent: $(".content-main"),

        dropdownPosition: 'below',
        language: {
            searching: function () {
                return "Đang tìm...";
            }
        },
        matcher: function (params, data) {
            if ($.trim(params.term) === '') return data;

            let term = removeVietnamese(params.term.toLowerCase());
            let text = removeVietnamese(data.text.toLowerCase());

            return text.indexOf(term) > -1 ? data : null;
        }
    });

    // Hàm thông báo nhỏ (toast)
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
                boxShadow: "0 2px 6px rgba(0,0,0,0.2)",
                zIndex: 9999,
                opacity: 0
            })
            .appendTo("body")
            .animate({ opacity: 1 }, 300)
            .delay(2000)
            .fadeOut(500, function () { $(this).remove(); });
    }

});
