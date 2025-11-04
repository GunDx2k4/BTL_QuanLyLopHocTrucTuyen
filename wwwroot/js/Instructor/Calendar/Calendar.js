document.addEventListener("DOMContentLoaded", function () {
    const calendarEl = document.getElementById("calendar");
    const courseInput = document.getElementById("courseSearch");
    const courseDropdown = document.getElementById("courseDropdown");
    const clearCourse = document.getElementById("clearCourse");
    const pageContainer = document.querySelector(".content-main");

    let currentCourseId = pageContainer.dataset.currentCourse || "all";
    let currentAssignmentId = null;

    // 🔹 Hiển thị mặc định tên khóa học đang chọn
    if (currentCourseId !== "all") {
        const currentLi = courseDropdown.querySelector(`[data-id='${currentCourseId}']`);
        if (currentLi) courseInput.value = currentLi.textContent.trim();
    } else {
        courseInput.value = "📚 Tất cả khóa học";
    }

    // ======= KHỞI TẠO CALENDAR =======
    const calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "dayGridMonth",
    locale: "vi",
    headerToolbar: {
        left: "prev,next today",
        center: "title",
        right: "dayGridMonth,timeGridWeek,timeGridDay"
    },
    buttonText: { today: "Hôm nay", month: "Tháng", week: "Tuần", day: "Ngày" },

    // ======= LOAD SỰ KIỆN =======
    events: function (fetchInfo, successCallback, failureCallback) {
        let url = "/Instructor/GetEvents";
        if (currentCourseId !== "all") url += `?courseId=${currentCourseId}`;

        fetch(url)
            .then(res => res.json())
            .then(data => {
                const truncateTitle = (text, limit = 40) =>
                    text.length > limit ? text.slice(0, limit) + "..." : text;

                // 🔹 Bảng màu cho từng loại
                const colorMap = {
                    "Bài học": "#0d6efd",       // xanh dương
                    "Bài kiểm tra": "#fd7e14", // cam
                    "Bài thi": "#dc3545"       // đỏ
                };

                const events = data
                    // 🧹 Chỉ giữ lại 3 loại cần hiển thị
                    .filter(ev =>
                        ev.type === "Bài học" ||
                        ev.type === "Bài kiểm tra" ||
                        ev.type === "Bài thi"
                    )
                    // 🧩 Map thành event cho FullCalendar
                    .map(ev => {
                        const color = colorMap[ev.type] || "#6c757d";

                        return {
                            id: ev.id,
                            title: truncateTitle(ev.title, 22),
                            // 🔸 Bài học hiển thị theo thời lượng, bài thi/kiểm tra hiển thị ở ngày kết thúc
                            start: ev.type === "Bài học"
                                ? ev.start
                                : new Date(ev.end).toISOString().split("T")[0],
                            end: ev.type === "Bài học" ? ev.end : null,
                            extendedProps: {
                                description: ev.description,
                                realStart: ev.start,
                                realEnd: ev.end,
                                type: ev.type
                            },
                            color,
                            textColor: "#fff"
                        };
                    });

                successCallback(events);
            })
            .catch(failureCallback);
        },

        eventDisplay: "block",

        // ======= CLICK HIỂN THỊ CHI TIẾT =======
        eventClick: function (info) {
            const props = info.event.extendedProps;

            document.getElementById("eventName").innerText = info.event.title;
            document.getElementById("eventType").innerText = props.type || "Không xác định";
            document.getElementById("eventStart").innerText =
                props.realStart ? new Date(props.realStart).toLocaleString("vi-VN") : "Không có";
            document.getElementById("eventEnd").innerText =
                props.realEnd ? new Date(props.realEnd).toLocaleString("vi-VN") : "Không có";
            document.getElementById("eventDesc").innerText = props.description || "Không có mô tả";

            // Cập nhật title modal
            document.getElementById("modalTitle").innerText =
                props.type === "Bài học" ? "📘 Chi tiết bài học" : "📄 Chi tiết bài tập";

            // Lưu thông tin để xử lý nút sửa
            currentEventId = info.event.id;
            currentEventType = props.type;

            new bootstrap.Modal(document.getElementById("eventModal")).show();
        }
    });

    calendar.render();

    // ======= LỌC KHÓA HỌC =======
    courseInput.addEventListener("focus", () => (courseDropdown.style.display = "block"));
    courseInput.addEventListener("input", function () {
        const keyword = this.value.toLowerCase();
        courseDropdown.querySelectorAll("li").forEach(li => {
            const text = li.textContent.toLowerCase();
            li.style.display = text.includes(keyword) ? "block" : "none";
        });
    });

    courseDropdown.addEventListener("click", function (e) {
        if (e.target.tagName === "LI") {
            const selectedId = e.target.dataset.id;
            const selectedName = e.target.textContent.trim();
            currentCourseId = selectedId;
            courseInput.value = selectedName;
            courseDropdown.style.display = "none";
            calendar.refetchEvents();
        }
    });

    clearCourse.addEventListener("click", function () {
        currentCourseId = "all";
        courseInput.value = "📚 Tất cả khóa học";
        calendar.refetchEvents();
    });

    document.addEventListener("click", function (e) {
        if (!courseDropdown.contains(e.target) && e.target !== courseInput) {
            courseDropdown.style.display = "none";
        }
    });

    // ======= NÚT SỬA (CHO CẢ HAI LOẠI) =======
    document.getElementById("btnEditEvent").addEventListener("click", function () {
        if (!currentEventId || !currentEventType) {
            alert("Không tìm thấy ID sự kiện!");
            return;
        }

        if (currentEventType === "Bài học") {
            window.location.href = `/Instructor/EditLessonRedirect/${currentEventId}`;
        } else {
            window.location.href = `/Instructor/EditAssignmentRedirect/${currentEventId}`;
        }
    });

    // ======= CẬP NHẬT LAYOUT =======
        const sidebarToggle = document.getElementById("toggleSidebar");
        const fullBtn = document.querySelector("[data-bs-toggle='fullscreen']"); // nếu có nút full screen
        let resizeTimeout = null;

        // Khi toggle sidebar hoặc fullscreen
        function forceCalendarResize() {
            // Xóa timer cũ nếu có
            if (resizeTimeout) clearTimeout(resizeTimeout);
            // Đợi DOM ổn định hẳn rồi mới update
            resizeTimeout = setTimeout(() => {
                calendar.updateSize();
            }, 600); // chờ 0.6s cho layout nở xong
        }

        if (sidebarToggle) {
            sidebarToggle.addEventListener("click", forceCalendarResize);
        }
        if (fullBtn) {
            fullBtn.addEventListener("click", forceCalendarResize);
        }

        // Khi resize màn hình
        window.addEventListener("resize", () => {
            forceCalendarResize();
        });

        // Trường hợp FullCalendar đang bị ẩn (display:none) → render lại khi hiện
        const observer = new MutationObserver(() => {
            if (calendarEl.offsetParent !== null) {
                calendar.updateSize();
            }
        });
        observer.observe(document.body, { attributes: true, childList: true, subtree: true });

});
