$(document).ready(function () {

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
