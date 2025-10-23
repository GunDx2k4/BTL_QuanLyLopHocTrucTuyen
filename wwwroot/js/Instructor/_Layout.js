document.addEventListener('DOMContentLoaded', () => {
    const toggleButton = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const layoutContainer = document.querySelector('.instructor-container');
    const overlay = document.querySelector('.sidebar-overlay');
    const mainContent = document.querySelector('.main-content');

    if (!toggleButton || !sidebar || !layoutContainer) return;

    const SIDEBAR_HIDDEN_CLASS = 'sidebar-hidden';
    const SIDEBAR_SHOW_CLASS = 'sidebar-show';

    const syncButtonIcon = (isHidden) => {
        const icon = toggleButton.querySelector('i');
        if (!icon) return;
        icon.classList.toggle('bi-layout-sidebar-inset-reverse', !isHidden);
        icon.classList.toggle('bi-layout-sidebar-inset', isHidden);
    };

    // 🧭 Toggle Sidebar khi nhấn nút
    toggleButton.addEventListener('click', (e) => {
        e.stopPropagation();
        const isMobile = window.innerWidth <= 768;

        if (isMobile) {
            const isOpen = layoutContainer.classList.contains(SIDEBAR_SHOW_CLASS);
            layoutContainer.classList.toggle(SIDEBAR_SHOW_CLASS, !isOpen);
            layoutContainer.classList.toggle(SIDEBAR_HIDDEN_CLASS, isOpen);
        } else {
            const isHidden = layoutContainer.classList.toggle(SIDEBAR_HIDDEN_CLASS);
            syncButtonIcon(isHidden);
        }
    });

    // 🖱️ Khi click overlay → ẩn sidebar (mobile)
    if (overlay) {
        overlay.addEventListener('click', () => {
            if (window.innerWidth <= 768) {
                layoutContainer.classList.remove(SIDEBAR_SHOW_CLASS);
                layoutContainer.classList.add(SIDEBAR_HIDDEN_CLASS);
            }
        });
    }

    // 🖱️ Khi click main-content → cũng ẩn sidebar (mobile)
    if (mainContent) {
        mainContent.addEventListener('click', () => {
            if (window.innerWidth <= 768 && layoutContainer.classList.contains(SIDEBAR_SHOW_CLASS)) {
                layoutContainer.classList.remove(SIDEBAR_SHOW_CLASS);
                layoutContainer.classList.add(SIDEBAR_HIDDEN_CLASS);
            }
        });
    }

    syncButtonIcon(layoutContainer.classList.contains(SIDEBAR_HIDDEN_CLASS));
});
