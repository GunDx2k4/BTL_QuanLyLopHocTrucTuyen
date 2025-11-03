$(document).ready(function () {

    // 🔠 Hàm loại bỏ dấu tiếng Việt
    function removeVietnameseTones(str) {
        return str
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")  // bỏ dấu thanh
            .replace(/đ/g, "d")
            .replace(/Đ/g, "D");
    }

    // 🔍 Tìm kiếm theo tên hoặc mô tả (có hỗ trợ dấu)
    $("#searchCourse").on("input", function () {
        const keyword = removeVietnameseTones($(this).val().toLowerCase().trim());

        $(".course-card").each(function () {
            const name = removeVietnameseTones(($(this).data("name") || "").toString().toLowerCase());
            const desc = removeVietnameseTones(($(this).data("desc") || "").toString().toLowerCase());

            $(this).toggle(name.includes(keyword) || desc.includes(keyword));
        });
    });

    // 🔽 Sắp xếp khóa học
        $("#sortCourse").on("change", function () {
        const type = $(this).val();
        const cards = $(".course-card").get();

        // ✅ Parse cả kiểu yyyyMMdd hoặc ISO yyyy-MM-ddTHH:mm:ss
        const parseDate = (val) => {
            if (!val) return new Date(0);
            const str = val.toString();
            // Nếu có "T" (ISO string) → parse trực tiếp
            if (str.includes("T")) return new Date(str);
            // Nếu là yyyyMMdd → chuyển thủ công
            if (/^\d{8}$/.test(str))
                return new Date(`${str.slice(0, 4)}-${str.slice(4, 6)}-${str.slice(6, 8)}`);
            return new Date(str); // fallback
        };

        cards.sort((a, b) => {
            const beginA = parseDate($(a).data("begin"));
            const beginB = parseDate($(b).data("begin"));
            const endA = parseDate($(a).data("end"));
            const endB = parseDate($(b).data("end"));
            const createdA = parseDate($(a).data("created"));
            const createdB = parseDate($(b).data("created"));
            const statusA = $(a).data("status");
            const statusB = $(b).data("status");

            switch (type) {
                case "begin":
                    return beginA - beginB; // sớm → muộn
                case "end":
                    return endA - endB;
                case "status":
                    const order = { "Chưa bắt đầu": 1, "Đang diễn ra": 2, "Đã kết thúc": 3 };
                    return (order[statusA] || 99) - (order[statusB] || 99);
                case "recent":
                default:
                    return createdB - createdA; // 🟢 mới nhất theo CreatedAt có thời gian
            }
        });

        console.log(`🔁 Sắp xếp theo: ${type}`);
        $("#courseGrid").empty().append(cards); // ✅ giữ nguyên event DOM
    });




    // 🧭 Chọn khóa học → lưu vào Claim và chuyển sang Lesson
    $(".course-header").on("click", function () {
        const courseId = $(this).closest(".course-card").data("id");
        if (!courseId) return;

        $.post("/Instructor/SelectCourse", { courseId: courseId })
            .done(() => {
                // ✅ Chuyển sang trang Lesson của giảng viên
                window.location.href = "/Instructor/Lesson";
            })
            .fail(() => {
                alert("❌ Không thể chọn khóa học. Vui lòng thử lại!");
            });
    });
     $(".course-footer a").on("click", function (e) {
        e.preventDefault();

        const btn = $(this);
        const courseCard = btn.closest(".course-card");
        const courseId = courseCard.data("id");
        const targetUrl = btn.attr("href"); // link gốc của nút

        if (!courseId || !targetUrl) return;

        $.post("/Instructor/SelectCourse", { courseId: courseId })
            .done(() => {
                window.location.href = targetUrl; // chuyển sang đúng trang
            })
            .fail(() => {
                Swal.fire({
                    icon: "error",
                    title: "Không thể chọn khóa học!",
                    text: "Vui lòng thử lại sau.",
                    confirmButtonColor: "#dc3545"
                });
            });
    });


    // ⚠️ Hiển thị popup nếu redirect từ trang con (chưa chọn khóa học)
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("requireCourse") === "true") {
        // dùng SweetAlert2 để hiển popup
        Swal.fire({
            icon: "warning",
            title: "Chưa chọn khóa học!",
            text: "Vui lòng chọn một khóa học trước khi truy cập nội dung này.",
            confirmButtonText: "Đã hiểu",
            confirmButtonColor: "#0d6efd"
        });
    }

});
