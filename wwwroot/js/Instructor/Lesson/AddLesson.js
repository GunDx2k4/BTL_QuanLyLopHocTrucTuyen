$(document).ready(function () {

    // Bắt sự kiện gửi form
    $("#lessonForm").on("submit", function (e) {
        e.preventDefault();
        if (!$(this).valid()) {
        showToast("⚠️ Vui lòng nhập đầy đủ thông tin trước khi lưu.", true);
        return;
    }

        const form = $(this);
        const url = form.attr("action");
        const data = form.serialize();
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: url,
            type: "POST",
            data: data,
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                showToast("✅ Thêm bài học thành công!");
                setTimeout(() => window.location.href = "/Instructor/Lesson", 800);
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("❌ Lỗi khi thêm bài học. Vui lòng thử lại!", true);
            }
        });
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
