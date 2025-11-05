// ====================== Instructor Setting ======================
$(document).ready(function () {
    const $form = $("#settingForm");
    const $alert = $("#alertPlaceholder");

    $form.on("submit", function (e) {
        e.preventDefault();

        const formData = $form.serialize();

        $.ajax({
            url: $form.attr("action"),
            type: "POST",
            data: formData,
            success: function () {
                showAlert("✅ Cập nhật tên hiển thị thành công!", "success");
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showAlert("❌ Đã xảy ra lỗi khi lưu thay đổi. Vui lòng thử lại.", "danger");
            }
        });
    });

    // 🔹 Hiển thị alert Bootstrap
    function showAlert(message, type) {
        const html = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        $alert.html(html);
        window.scrollTo({ top: 0, behavior: "smooth" });
    }
});
