
// Ẩn/hiện input số tháng
$("#planType").on("change", function () {
    const value = parseInt($(this).val());
    if (value >= 1) { // Basic hoặc Premium
        $("#monthContainer").show();
    } else {
        $("#monthContainer").hide();
        $("#expireInfo").hide();
    }
});

function parseVNDate(dateStr) {
    if (!dateStr || dateStr === 'null') return null;
    const [day, month, year] = dateStr.split('/').map(Number);
    if (isNaN(day) || isNaN(month) || isNaN(year)) return null;
    return new Date(year, month - 1, day); // JS month bắt đầu từ 0
}
$(document).ready(function () {
    $("#monthCount").val(1).trigger("input");
});

$("#monthCount").on("input", function () {
    const months = parseInt($(this).val());
    const baseDate = parseVNDate(window.tenantEndTime) || new Date();
    if (!isNaN(months) && months > 0) {

        const expire = new Date(baseDate);
        expire.setMonth(baseDate.getMonth() + months);

        const formatted = expire.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });

        $("#expireInfo")
            .text(`Hết hạn vào: ${formatted}`)
            .show();
    } else {
        const formatted = baseDate.toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        });
        $("#expireInfo")
            .text(`Hết hạn vào: ${formatted}`)
            .show();
    }
});


$("#formAddPlan").on("submit", (e) => {
    e.preventDefault();

    fetch(`/api/tenants/${tenantId}/plan`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            newPlan: parseInt($("#planType").val()),
            durationInMonths: parseInt($("#planType").val()) >= 1 ? parseInt($("#monthCount").val()) : 0
        })
    }).then(response => {
        if (!response.ok) {
            showModal("Lỗi: " + response.statusText);
            $("#modalPlan").modal('hide');
            return;
        }
        $("#modalPlan").modal('hide');
        showModal("Cập nhật gói thành công!", "/");
    }).catch(err => {
        $("#alert").text("Đã có lỗi xảy ra: " + err.message).show();
    });

});

$("#formAddInstructor").on("submit", (e) => {
    e.preventDefault();

    fetch(`/api/users/instructors`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            fullName: $("#instructorName").val(),
            email: $("#instructorEmail").val(),
            password: $("#instructorPassword").val()
        })
    }).then(response => {
        if (!response.ok) {
            showModal("Lỗi: " + response.statusText);
            $("#modalInstructor").modal('hide');
            return;
        }
        $("#modalInstructor").modal('hide');
        showModal("Thêm giảng viên thành công!", "/");
    }).catch(err => {
        $("#alert").text("Đã có lỗi xảy ra: " + err.message).show();
    });
});

$("#formAddStudent").on("submit", (e) => {
    e.preventDefault();

    fetch(`/api/users/students`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            fullName: $("#studentName").val(),
            email: $("#studentEmail").val(),
            password: $("#studentPassword").val()
        })
    }).then(response => {
        if (!response.ok) {
            showModal("Lỗi: " + response.statusText);
            $("#modalStudent").modal('hide');
            return;
        }
        $("#modalStudent").modal('hide');
        showModal("Thêm học viên thành công!", "/");
    }).catch(err => {
        $("#alert").text("Đã có lỗi xảy ra: " + err.message).show();
    });
});

$("#modalCourse").on("show.bs.modal", () => {
    const $instructorSelect = $("#instructorId");

    fetch("/api/users/instructors")
        .then(res => {
            if (!res.ok) throw new Error("Không thể tải danh sách giảng viên.");
            return res.json();
        })
        .then(data => {
            if (data.length === 0) {
                $instructorSelect.html(`<option value="">-- Không có giảng viên nào --</option>`);
                return;
            }
            data.forEach(item => {
                $instructorSelect.append(
                    `<option value="${item.id}">${item.fullName} (${item.email})</option>`
                );
            });
        })
        .catch(err => {
            $instructorSelect.html(`<option value="">-- Lỗi tải dữ liệu --</option>`);
            console.error(err);
        });
});


$("#formAddCourse").on("submit", (e) => {
    e.preventDefault();

    fetch(`/api/courses`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            name: $("#courseName").val(),
            description: $("#courseDescription").val(),
            beginTime: $("#beginTime").val(),
            endTime: $("#endTime").val(),
            instructorId: $("#instructorId").val()
        })
    }).then(response => {
        if (!response.ok) {
            showModal("Lỗi: " + response.statusText);
            $("#modalCourse").modal('hide');
            return;
        }
        $("#modalCourse").modal('hide');
        showModal("Thêm khoá học thành công!", "/");
    }).catch(err => {
        $("#alertCourse").text("Đã có lỗi xảy ra: " + err.message).show();
    });
});