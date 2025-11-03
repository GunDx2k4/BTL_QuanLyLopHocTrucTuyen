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
                    const events = data.map(ev => {
                        // Nếu là bài tập → chỉ hiển thị ở ngày kết thúc
                        const isAssignment =
                            ev.type?.toLowerCase().includes("bài") ||
                            ev.type?.toLowerCase().includes("assignment");

                        return {
                            id: ev.id,
                            title: ev.title,
                            start: isAssignment
                                ? ev.end // 🔹 chỉ hiện ở ngày kết thúc
                                : ev.start,
                            end: isAssignment ? null : ev.end,
                            extendedProps: {
                                description: ev.description,
                                realStart: ev.start,
                                realEnd: ev.end,
                                type: ev.type
                            },
                            color: isAssignment ? "#dc3545" : "#0d6efd", // đỏ = bài tập, xanh = bài học
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

            currentAssignmentId = info.event.id;
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

    // ======= NÚT SỬA BÀI TẬP =======
    document.getElementById("btnEditAssignment").addEventListener("click", function () {
        if (currentAssignmentId) {
            window.location.href = `/Instructor/EditAssignmentRedirect/${currentAssignmentId}`;
        } else {
            alert("Không tìm thấy ID bài tập!");
        }
    });

    // ======= CẬP NHẬT LAYOUT =======
    const sidebarToggle = document.getElementById("toggleSidebar");
    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", () => {
            setTimeout(() => calendar.updateSize(), 400);
        });
    }
    window.addEventListener("resize", () => calendar.updateSize());
});
