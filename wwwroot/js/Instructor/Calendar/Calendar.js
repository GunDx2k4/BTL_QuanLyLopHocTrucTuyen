document.addEventListener("DOMContentLoaded", function () {
    const calendarEl = document.getElementById("calendar");
    const courseInput = document.getElementById("courseSearch");
    const courseDropdown = document.getElementById("courseDropdown");
    const clearCourse = document.getElementById("clearCourse");
    const pageContainer = document.querySelector(".content-main");

    let currentCourseId = pageContainer.dataset.currentCourse || "all";
    let currentEventId = null;
    let currentEventType = null;

    // ======= HIỂN THỊ MẶC ĐỊNH =======
    if (currentCourseId !== "all") {
        const currentLi = courseDropdown.querySelector(`[data-id='${currentCourseId}']`);
        if (currentLi) courseInput.value = currentLi.textContent.trim();
    } else {
        courseInput.value = "📚 Tất cả khóa học";
    }

    // ======= KHỞI TẠO FULLCALENDAR =======
    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",
        locale: "vi",
        headerToolbar: {
            left: "prev,next today",
            center: "title",
            right: "dayGridMonth,timeGridWeek,timeGridDay"
        },
        buttonText: {
            today: "Hôm nay",
            month: "Tháng",
            week: "Tuần",
            day: "Ngày"
        },

        // ======= LOAD SỰ KIỆN =======
        events: function (fetchInfo, successCallback, failureCallback) {
            let url = "/Instructor/GetEvents";
            if (currentCourseId !== "all") url += `?courseId=${currentCourseId}`;

            fetch(url)
                .then(res => res.json())
                .then(data => {
                    const truncateTitle = (text, limit = 40) =>
                        text.length > limit ? text.slice(0, limit) + "..." : text;

                    const colorMap = {
                        "Bài học": "#0d6efd",
                        "Bài kiểm tra": "#fd7e14",
                        "Bài thi": "#dc3545"
                    };

                    const events = data
                        .filter(ev =>
                            ev.type === "Bài học" ||
                            ev.type === "Bài kiểm tra" ||
                            ev.type === "Bài thi"
                        )
                        .map(ev => {
                            const color = colorMap[ev.type] || "#6c757d";
                            return {
                                id: ev.id,
                                title: truncateTitle(ev.title, 22),
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
            document.getElementById("modalTitle").innerText =
                props.type === "Bài học" ? "📘 Chi tiết bài học" : "📄 Chi tiết bài tập";

            currentEventId = info.event.id;
            currentEventType = props.type;
            new bootstrap.Modal(document.getElementById("eventModal")).show();
        }
    });

    calendar.render();

   // =====================================================
    // 🔍 Dropdown: Tìm kiếm & Chọn khóa học — Fix hoàn chỉnh
    // =====================================================

    // 🧱 Thêm item "Không tìm thấy"
    const noResultItem = document.createElement("li");
    noResultItem.className = "list-group-item text-muted text-center fst-italic";
    noResultItem.textContent = "Không tìm thấy khóa học nào";
    noResultItem.style.display = "none";
    courseDropdown.appendChild(noResultItem);

    // 🔸 Hàm lọc danh sách
    function filterCourseItems(keyword) {
        const lowerKeyword = keyword.toLowerCase().trim();
        let visibleCount = 0;

        courseDropdown.querySelectorAll("li").forEach(li => {
            if (li === noResultItem) return;
            const text = li.textContent.toLowerCase();
            const match = text.includes(lowerKeyword);
            li.style.display = match ? "block" : "none";
            if (match) visibleCount++;
        });

        noResultItem.style.display = visibleCount === 0 ? "block" : "none";
        courseDropdown.style.display = "block";
    }

    // 🔹 Khi click vào input → luôn mở dropdown
    courseInput.addEventListener("focus", function () {
        courseDropdown.querySelectorAll("li").forEach(li => li.style.display = "block");
        courseDropdown.style.display = "block";
    });

    // 🔹 Khi nhập từ khóa
    courseInput.addEventListener("input", function () {
        filterCourseItems(this.value);
    });

    // 🔹 Khi chọn khóa học
    courseDropdown.addEventListener("click", function (e) {
        if (e.target.tagName === "LI" && e.target !== noResultItem) {
            const selectedId = e.target.dataset.id;
            const selectedName = e.target.textContent.trim();

            currentCourseId = selectedId;
            courseInput.value = selectedName;
            courseDropdown.style.display = "none";
            clearCourse.style.display = "inline";
            calendar.refetchEvents();
        }
    });

    // 🔹 Khi bấm nút ❌ Clear
    clearCourse.addEventListener("click", function (e) {
        e.stopPropagation(); // Ngăn document.click ẩn dropdown

        currentCourseId = "all";
        courseInput.value = "";
        clearCourse.style.display = "none";

        // Hiển thị lại toàn bộ danh sách
        courseDropdown.querySelectorAll("li").forEach(li => li.style.display = "block");

        // Hiện dropdown ngay
        courseDropdown.style.display = "block";
        courseInput.focus();

        // Refetch lịch sau 1 chút
        setTimeout(() => calendar.refetchEvents(), 150);
    });

    // 🔹 Click ra ngoài → ẩn dropdown & bỏ focus khỏi input
    document.addEventListener("mousedown", function (e) {
        const isInside =
            courseDropdown.contains(e.target) ||
            e.target === courseInput ||
            e.target === clearCourse;

        // Nếu click ra ngoài, ẩn dropdown sau 150ms
        if (!isInside) {
            setTimeout(() => {
                courseDropdown.style.display = "none";
                courseInput.blur(); // 👈 Bỏ con trỏ nháy
            }, 150);
        }
    });




    // =====================================================
    // 🛠️ NÚT SỬA SỰ KIỆN
    // =====================================================
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

    // =====================================================
    // 🔄 CẬP NHẬT LAYOUT FULLCALENDAR KHI RESIZE / TOGGLE
    // =====================================================
    const sidebarToggle = document.getElementById("toggleSidebar");
    const fullBtn = document.querySelector("[data-bs-toggle='fullscreen']");
    let resizeTimeout = null;

    function forceCalendarResize() {
        if (resizeTimeout) clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            calendar.updateSize();
        }, 600);
    }

    if (sidebarToggle) sidebarToggle.addEventListener("click", forceCalendarResize);
    if (fullBtn) fullBtn.addEventListener("click", forceCalendarResize);
    window.addEventListener("resize", forceCalendarResize);

    const observer = new MutationObserver(() => {
        if (calendarEl.offsetParent !== null) {
            calendar.updateSize();
        }
    });
    observer.observe(document.body, { attributes: true, childList: true, subtree: true });
});
