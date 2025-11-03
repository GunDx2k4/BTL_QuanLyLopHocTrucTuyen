$(document).ready(function () {
    const $search = $("#courseSearch");
    const $dropdown = $("#courseDropdown");
    const $clear = $("#clearCourse");

    // 🔍 Gõ tìm kiếm trong danh sách
    $search.on("input focus", function () {
        const keyword = $(this).val().toLowerCase().trim();
        let hasResult = false;

        $dropdown.children("li").each(function () {
            const text = $(this).text().toLowerCase();
            const match = text.includes(keyword);
            $(this).toggle(match);
            if (match) hasResult = true;
        });

        $dropdown.toggle(hasResult);
    });

    // ✅ Chọn khóa học
    $dropdown.on("click", "li", function () {
        const name = $(this).text();
        const id = $(this).data("id");

        $search.val(name);
        $dropdown.hide();
        $clear.show();

        // Gọi API chọn khóa học
        $.ajax({
            url: `/Instructor/SelectCourse`,
            type: "POST",
            data: { courseId: id },
            success: function () {
                location.reload();
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert("Không thể chọn khóa học.");
            }
        });
    });

    // ❌ Nút xóa
    $clear.on("click", function () {
        $search.val("");
        $clear.hide();
        $dropdown.show();
    });

    // Ẩn khi click ra ngoài
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".filter-course").length) {
            $dropdown.hide();
        }
    });

    // 📊 Biểu đồ demo
    const ctx1 = document.getElementById("submissionChart");
    const ctx2 = document.getElementById("rateChart");

    new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: ['Bài 1', 'Bài 2', 'SQL', 'Java'],
            datasets: [{
                label: 'Đã nộp',
                data: [35, 30, 27, 20],
                backgroundColor: '#0d6efd',
                borderRadius: 8
            }, {
                label: 'Chưa nộp',
                data: [5, 10, 3, 10],
                backgroundColor: '#adb5bd',
                borderRadius: 8
            }]
        },
        options: { plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
    });

    new Chart(ctx2, {
        type: 'doughnut',
        data: {
            labels: ['Đã nộp', 'Chưa nộp'],
            datasets: [{ data: [92, 28], backgroundColor: ['#198754', '#dee2e6'], hoverOffset: 8 }]
        },
        options: { plugins: { legend: { position: 'bottom' } } }
    });
});
