$(document).ready(function () {

    $("#lessonForm").on("submit", function (e) {
        e.preventDefault();

        const form = $(this);
        const url = form.attr("action");
        const data = form.serialize();
        const token = $('input[name="__RequestVerificationToken"]').val();

        if (!form.valid()) {
            showToast("⚠️ Vui lòng nhập đầy đủ thông tin trước khi lưu.", true);
            return;
        }

        $.ajax({
            url: url,
            type: "POST",
            data: data,
            headers: { "RequestVerificationToken": token },
            success: function (res) {
                if (res.success) {
                    showToast("✅ Thêm bài học thành công!");
                    setTimeout(() => (window.location.href = "/Instructor/Lesson"), 800);
                } else {
                    // ❗ Nếu trùng giờ hoặc lỗi logic khác
                    showPopup(res.message || "❌ Có lỗi xảy ra!");
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showPopup("⚠️ Lỗi khi thêm bài học. Vui lòng thử lại!");
            }
        });
    });

    // ====== 🔔 Toast góc màn hình (dành cho thành công) ======
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
                opacity: 0,
                fontWeight: "500",
            })
            .appendTo("body")
            .animate({ opacity: 1 }, 250)
            .delay(2200)
            .fadeOut(400, function () {
                $(this).remove();
            });
    }

    // ====== 🧩 Popup giữa màn hình ======
    function showPopup(message) {
        // Nếu đã có popup rồi → xóa trước
        $(".lesson-popup-overlay").remove();

        const overlay = $(`
            <div class="lesson-popup-overlay">
                <div class="lesson-popup">
                    <h5 class="fw-bold mb-3"><i class="bi bi-exclamation-circle text-danger me-2"></i>Thông báo</h5>
                    <p class="popup-message mb-4">${message}</p>
                    <div class="popup-buttons d-flex justify-content-center gap-3">
                        <button id="viewScheduleBtn" class="btn btn-primary">
                            <i class="bi bi-calendar-week me-1"></i>Xem lịch
                        </button>
                        <button id="closePopupBtn" class="btn btn-outline-secondary">
                            Đóng
                        </button>
                    </div>
                </div>
            </div>
        `).appendTo("body");

        // CSS inline cơ bản (có thể chuyển qua file .css)
        $(".lesson-popup-overlay").css({
            position: "fixed",
            top: 0,
            left: 0,
            width: "100vw",
            height: "100vh",
            background: "rgba(0,0,0,0.4)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 10000
        });

        $(".lesson-popup").css({
            background: "#fff",
            padding: "24px 32px",
            borderRadius: "10px",
            width: "420px",
            maxWidth: "90%",
            boxShadow: "0 4px 12px rgba(0,0,0,0.2)",
            textAlign: "center",
            animation: "fadeIn 0.3s ease"
        });

        // Nút đóng
        $("#closePopupBtn").on("click", function () {
            $(".lesson-popup-overlay").fadeOut(200, function () {
                $(this).remove();
            });
        });

        // Nút xem lịch
        $("#viewScheduleBtn").on("click", function () {
            window.location.href = "/Instructor/Calendar";
        });
    }
});
