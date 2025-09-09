// Sidebar toggle (mobile)
(function () {
    const toggle = document.querySelector('[data-admin-toggle]');
    const sidebar = document.querySelector('.admin-sidebar');
    const backdrop = document.querySelector('.sidebar-backdrop');
    if (!toggle || !sidebar || !backdrop) return;
    function open() { sidebar.classList.add('open'); backdrop.classList.add('show'); }
    function close() { sidebar.classList.remove('open'); backdrop.classList.remove('show'); }
    toggle.addEventListener('click', open);
    backdrop.addEventListener('click', close);
})();

// Bootstrap Toast (TempData'dan)
(function () {
    const toasts = document.querySelectorAll('.toast');
    [...toasts].forEach(t => new bootstrap.Toast(t, { delay: 3500 }).show());
})();

// Hero preview
window.bindHeroPreview = (inputId, imgId) => {
    const file = document.getElementById(inputId);
    const preview = document.getElementById(imgId);
    if (!file || !preview) return;
    file.addEventListener('change', () => {
        const f = file.files?.[0]; if (!f) return;
        const url = URL.createObjectURL(f);
        preview.src = url;
    });
};

// Galeri tek tek kaldır (client-side)
window.removeGalleryItem = (btn) => {
    const box = btn.closest('.gallery-item');
    if (box) box.remove();
};
