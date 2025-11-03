

const StudentModule = (function () {
    'use strict';

    // ===== NOTIFICATION SYSTEM =====
    const Notification = {
        container: null,

        init() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'notification-container';
                this.container.style.cssText = `
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    max-width: 400px;
                `;
                document.body.appendChild(this.container);
            }
        },

        show(message, type = 'info', duration = 5000) {
            this.init();

            const icons = {
                success: '✅',
                error: '❌',
                warning: '⚠️',
                info: 'ℹ️'
            };

            const colors = {
                success: '#28a745',
                error: '#dc3545',
                warning: '#ffc107',
                info: '#17a2b8'
            };

            const notification = document.createElement('div');
            notification.style.cssText = `
                background: white;
                border-left: 4px solid ${colors[type]};
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                padding: 16px 20px;
                margin-bottom: 10px;
                display: flex;
                align-items: center;
                gap: 12px;
                animation: slideIn 0.3s ease-out;
                max-width: 100%;
            `;

            notification.innerHTML = `
                <span style="font-size: 24px;">${icons[type]}</span>
                <span style="flex: 1; color: #333;">${message}</span>
                <button onclick="this.parentElement.remove()" 
                        style="background: none; border: none; font-size: 20px; 
                               cursor: pointer; color: #999; line-height: 1;">×</button>
            `;

            this.container.appendChild(notification);

            if (duration > 0) {
                setTimeout(() => {
                    notification.style.animation = 'slideOut 0.3s ease-out';
                    setTimeout(() => notification.remove(), 300);
                }, duration);
            }

            return notification;
        },

        success(message, duration) {
            return this.show(message, 'success', duration);
        },

        error(message, duration) {
            return this.show(message, 'error', duration);
        },

        warning(message, duration) {
            return this.show(message, 'warning', duration);
        },

        info(message, duration) {
            return this.show(message, 'info', duration);
        }
    };

    // ===== MODAL SYSTEM =====
    const Modal = {
        show(config) {
            const { title, message, type = 'confirm', onConfirm, onCancel } = config;

            const overlay = document.createElement('div');
            overlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(0,0,0,0.5);
                z-index: 10000;
                display: flex;
                align-items: center;
                justify-content: center;
                animation: fadeIn 0.2s ease-out;
            `;

            const modal = document.createElement('div');
            modal.style.cssText = `
                background: white;
                border-radius: 12px;
                padding: 24px;
                max-width: 500px;
                width: 90%;
                box-shadow: 0 8px 32px rgba(0,0,0,0.2);
                animation: scaleIn 0.3s ease-out;
            `;

            const icons = {
                confirm: '❓',
                warning: '⚠️',
                danger: '🚨',
                info: 'ℹ️'
            };

            modal.innerHTML = `
                <div style="text-align: center; margin-bottom: 20px;">
                    <div style="font-size: 48px; margin-bottom: 16px;">${icons[type]}</div>
                    <h3 style="margin: 0 0 12px 0; color: #333;">${title}</h3>
                    <p style="color: #666; margin: 0;">${message}</p>
                </div>
                <div style="display: flex; gap: 12px; justify-content: center;">
                    <button class="modal-cancel" style="
                        padding: 10px 24px;
                        border: 1px solid #ddd;
                        background: white;
                        border-radius: 6px;
                        cursor: pointer;
                        font-size: 16px;
                        transition: all 0.2s;
                    ">Hủy</button>
                    <button class="modal-confirm" style="
                        padding: 10px 24px;
                        border: none;
                        background: #007bff;
                        color: white;
                        border-radius: 6px;
                        cursor: pointer;
                        font-size: 16px;
                        transition: all 0.2s;
                    ">Xác nhận</button>
                </div>
            `;

            overlay.appendChild(modal);
            document.body.appendChild(overlay);

            // Add hover effects
            const confirmBtn = modal.querySelector('.modal-confirm');
            const cancelBtn = modal.querySelector('.modal-cancel');

            confirmBtn.addEventListener('mouseenter', () => {
                confirmBtn.style.background = '#0056b3';
            });
            confirmBtn.addEventListener('mouseleave', () => {
                confirmBtn.style.background = '#007bff';
            });

            cancelBtn.addEventListener('mouseenter', () => {
                cancelBtn.style.background = '#f8f9fa';
            });
            cancelBtn.addEventListener('mouseleave', () => {
                cancelBtn.style.background = 'white';
            });

            // Event handlers
            const close = () => {
                overlay.style.animation = 'fadeOut 0.2s ease-out';
                setTimeout(() => overlay.remove(), 200);
            };

            confirmBtn.addEventListener('click', () => {
                if (onConfirm) onConfirm();
                close();
            });

            cancelBtn.addEventListener('click', () => {
                if (onCancel) onCancel();
                close();
            });

            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) {
                    if (onCancel) onCancel();
                    close();
                }
            });

            return { close };
        }
    };

    // ===== LOADING OVERLAY =====
    const Loading = {
        overlay: null,

        show(message = 'Đang xử lý...') {
            if (!this.overlay) {
                this.overlay = document.createElement('div');
                this.overlay.style.cssText = `
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0,0,0,0.7);
                    z-index: 10001;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    animation: fadeIn 0.2s ease-out;
                `;

                this.overlay.innerHTML = `
                    <div style="
                        background: white;
                        padding: 32px;
                        border-radius: 12px;
                        text-align: center;
                        box-shadow: 0 8px 32px rgba(0,0,0,0.2);
                    ">
                        <div class="spinner" style="
                            border: 4px solid #f3f3f3;
                            border-top: 4px solid #007bff;
                            border-radius: 50%;
                            width: 50px;
                            height: 50px;
                            animation: spin 1s linear infinite;
                            margin: 0 auto 16px;
                        "></div>
                        <p style="margin: 0; color: #333; font-size: 16px;">${message}</p>
                    </div>
                `;
            }

            document.body.appendChild(this.overlay);
            return this;
        },

        hide() {
            if (this.overlay) {
                this.overlay.style.animation = 'fadeOut 0.2s ease-out';
                setTimeout(() => {
                    if (this.overlay && this.overlay.parentNode) {
                        this.overlay.remove();
                        this.overlay = null;
                    }
                }, 200);
            }
            return this;
        }
    };

    // ===== COURSE MANAGEMENT =====
    const CourseManager = {
        async enrollCourse(courseId, courseName) {
            const confirmed = await new Promise((resolve) => {
                Modal.show({
                    title: 'Xác nhận đăng ký',
                    message: `Bạn có chắc muốn đăng ký khóa học "${courseName}"?`,
                    type: 'confirm',
                    onConfirm: () => resolve(true),
                    onCancel: () => resolve(false)
                });
            });

            if (!confirmed) return;

            Loading.show('Đang đăng ký khóa học...');

            try {
                const response = await fetch('/Student/EnrollCourse', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: new URLSearchParams({
                        courseId: courseId,
                        __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    })
                });

                Loading.hide();

                if (response.ok) {
                    Notification.success('Đăng ký khóa học thành công!');
                    setTimeout(() => {
                        window.location.href = '/Student/MyCourses';
                    }, 1500);
                } else {
                    Notification.error('Bạn đã đăng ký khóa học này rồi!');
                }
            } catch (error) {
                Loading.hide();
                console.error('Enrollment error:', error);
                Notification.error('Có lỗi xảy ra khi đăng ký khóa học!');
            }
        },

        async dropCourse(enrollmentId, courseName) {
            const confirmed = await new Promise((resolve) => {
                Modal.show({
                    title: 'Cảnh báo!',
                    message: `Bạn có chắc chắn muốn hủy đăng ký khóa học "${courseName}"? Bạn sẽ mất quyền truy cập vào tài liệu và bài tập.`,
                    type: 'danger',
                    onConfirm: () => resolve(true),
                    onCancel: () => resolve(false)
                });
            });

            if (!confirmed) return;

            Loading.show('Đang hủy đăng ký...');

            try {
                const response = await fetch('/Student/DropCourse', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: new URLSearchParams({
                        enrollmentId: enrollmentId,
                        __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    })
                });

                Loading.hide();

                if (response.ok) {
                    Notification.success('Hủy đăng ký khóa học thành công!');
                    setTimeout(() => {
                        window.location.href = '/Student/MyCourses';
                    }, 1500);
                } else {
                    Notification.error('Không thể hủy đăng ký khóa học!');
                }
            } catch (error) {
                Loading.hide();
                console.error('Drop course error:', error);
                Notification.error('Có lỗi xảy ra khi hủy đăng ký!');
            }
        }
    };

    // ===== ASSIGNMENT SUBMISSION =====
    const AssignmentManager = {
        previewFile(input) {
            const file = input.files[0];
            if (!file) return;

            const fileInfo = document.getElementById('fileInfo');
            const fileName = document.getElementById('fileName');
            const fileSize = document.getElementById('fileSize');

            fileName.textContent = file.name;
            fileSize.textContent = this.formatFileSize(file.size);
            fileInfo.style.display = 'block';

            // Validate file size (50MB)
            if (file.size > 50 * 1024 * 1024) {
                Notification.error('File quá lớn! Kích thước tối đa là 50MB.');
                input.value = '';
                fileInfo.style.display = 'none';
            } else {
                Notification.success(`File "${file.name}" đã được chọn!`, 3000);
            }
        },

        formatFileSize(bytes) {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
        },

        async handleSubmit(event) {
            event.preventDefault();

            const form = event.target;
            const file = document.getElementById('file')?.files[0];
            const confirmCheck = document.getElementById('confirmCheck');

            // Validate
            if (!file) {
                Notification.error('Vui lòng chọn file để nộp!');
                return false;
            }

            if (!confirmCheck?.checked) {
                Notification.error('Vui lòng tick xác nhận trước khi nộp!');
                return false;
            }

            // Confirm submission
            const confirmed = await new Promise((resolve) => {
                Modal.show({
                    title: 'Xác nhận nộp bài',
                    message: `Bạn có chắc chắn muốn nộp bài tập này?\nFile: ${file.name}`,
                    type: 'confirm',
                    onConfirm: () => resolve(true),
                    onCancel: () => resolve(false)
                });
            });

            if (!confirmed) return false;

            // Submit
            const submitBtn = document.getElementById('submitBtn');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang nộp...';
            }

            Loading.show('Đang tải file lên...');

            try {
                const formData = new FormData(form);
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData
                });

                Loading.hide();

                if (response.ok) {
                    Notification.success('Nộp bài tập thành công!');
                    setTimeout(() => {
                        window.location.href = '/Student/MySubmissions';
                    }, 1500);
                } else {
                    throw new Error('Submission failed');
                }
            } catch (error) {
                Loading.hide();
                console.error('Submission error:', error);
                Notification.error('Có lỗi xảy ra khi nộp bài tập!');

                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '✅ Nộp bài';
                }
            }

            return false;
        },

        downloadFile(submissionId) {
            Loading.show('Đang tải file...');

            window.location.href = `/Student/DownloadFile/${submissionId}`;

            setTimeout(() => {
                Loading.hide();
                Notification.success('File đã được tải xuống!');
            }, 2000);
        }
    };

    // ===== REPORT EXPORT =====
    const ReportManager = {
        async exportReport(format = 'pdf') {
            Loading.show(`Đang xuất báo cáo ${format.toUpperCase()}...`);

            try {
                const response = await fetch(`/Student/Reports/Export?format=${format}`);

                if (response.ok) {
                    const blob = await response.blob();
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `BaoCaoHocTap_${new Date().toISOString().split('T')[0]}.${format}`;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);

                    Loading.hide();
                    Notification.success('Xuất báo cáo thành công!');
                } else {
                    throw new Error('Export failed');
                }
            } catch (error) {
                Loading.hide();
                console.error('Export error:', error);
                Notification.warning('Chức năng xuất báo cáo sẽ khả dụng sau khi kết nối Backend!');
            }
        }
    };

    // ===== ADD CSS ANIMATIONS =====
    const addStyles = () => {
        if (!document.getElementById('student-module-styles')) {
            const style = document.createElement('style');
            style.id = 'student-module-styles';
            style.textContent = `
                @keyframes slideIn {
                    from {
                        transform: translateX(400px);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }

                @keyframes slideOut {
                    from {
                        transform: translateX(0);
                        opacity: 1;
                    }
                    to {
                        transform: translateX(400px);
                        opacity: 0;
                    }
                }

                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }

                @keyframes fadeOut {
                    from { opacity: 1; }
                    to { opacity: 0; }
                }

                @keyframes scaleIn {
                    from {
                        transform: scale(0.9);
                        opacity: 0;
                    }
                    to {
                        transform: scale(1);
                        opacity: 1;
                    }
                }

                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }

                /* Smooth transitions for buttons */
                button {
                    transition: all 0.2s ease !important;
                }

                button:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                }

                button:active {
                    transform: translateY(0);
                }
            `;
            document.head.appendChild(style);
        }
    };

    // ===== INITIALIZATION =====
    const init = () => {
        addStyles();

        // Auto-show TempData messages
        const successMsg = document.querySelector('[data-success-message]');
        const errorMsg = document.querySelector('[data-error-message]');
        const warningMsg = document.querySelector('[data-warning-message]');
        const infoMsg = document.querySelector('[data-info-message]');

        if (successMsg) Notification.success(successMsg.dataset.successMessage);
        if (errorMsg) Notification.error(errorMsg.dataset.errorMessage);
        if (warningMsg) Notification.warning(warningMsg.dataset.warningMessage);
        if (infoMsg) Notification.info(infoMsg.dataset.infoMessage);

        console.log('✅ Student Module initialized');
    };

    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // ===== PUBLIC API =====
    return {
        notify: Notification,
        modal: Modal,
        loading: Loading,
        course: CourseManager,
        assignment: AssignmentManager,
        report: ReportManager
    };
})();

// ===== GLOBAL FUNCTIONS FOR INLINE ONCLICK =====
function enrollCourse(courseId, courseName) {
    StudentModule.course.enrollCourse(courseId, courseName);
}

function dropCourse(enrollmentId, courseName) {
    StudentModule.course.dropCourse(enrollmentId, courseName);
}

function downloadFile(submissionId) {
    StudentModule.assignment.downloadFile(submissionId);
}

function exportReport(format) {
    StudentModule.report.exportReport(format);
}

function previewFile(input) {
    StudentModule.assignment.previewFile(input);
}

// Make StudentModule globally accessible
window.StudentModule = StudentModule;