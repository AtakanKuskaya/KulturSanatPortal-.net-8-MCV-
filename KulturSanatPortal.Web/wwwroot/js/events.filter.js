/*! Events filter (calendar + categories + today) — final */
(function () {
    if (window.__EVT_FILTER__) return; window.__EVT_FILTER__ = true;

    const listBox = document.getElementById('evtList');

    function buildUrl(dateISO, catId) {
        const u = new URL(window.location.href);
        if (dateISO) u.searchParams.set('date', dateISO); else u.searchParams.delete('date');
        if (catId) u.searchParams.set('categoryId', catId); else u.searchParams.delete('categoryId');
        return u;
    }

    async function loadList(dateISO, catId) {
        const url = new URL(window.location.origin + '/Events/ListPartial');
        if (dateISO) url.searchParams.set('date', dateISO);
        if (catId) url.searchParams.set('categoryId', catId);

        if (listBox) listBox.classList.add('opacity-50');
        const html = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } }).then(r => r.text());
        if (listBox) {
            listBox.innerHTML = html;
            listBox.classList.remove('opacity-50');
        }
        history.pushState({}, '', buildUrl(dateISO, catId));
    }

    // MiniCalendar: gün seçimi
    document.addEventListener('miniCalendar:select', (e) => {
        const iso = e.detail?.date; // yyyy-mm-dd
        const cat = document.querySelector('input[name="categoryId"]:checked')?.value || '';
        if (iso) loadList(iso, cat);
    });

    // Takvimde server-side link varsa yakala
    document.addEventListener('click', (e) => {
        const a = e.target.closest('.calendar-mini a.day-select'); // bizim link
        if (!a) return;
        e.preventDefault();
        const iso = a.getAttribute('data-date');
        const cat = document.querySelector('input[name="categoryId"]:checked')?.value || '';
        loadList(iso, cat);
    });

    // Kategori değişimi
    document.addEventListener('change', (e) => {
        const r = e.target.closest('input[name="categoryId"]');
        if (!r) return;
        const currentDate = (new URL(window.location.href)).searchParams.get('date') || '';
        loadList(currentDate, r.value || '');
    });

    // "Bugün" butonu — yönlendirme yok
    document.addEventListener('click', (e) => {
        const a = e.target.closest('a[data-today]');
        if (!a) return;
        e.preventDefault();
        const d = new Date();
        const iso = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
        const cat = document.querySelector('input[name="categoryId"]:checked')?.value || '';
        loadList(iso, cat);
    });
})();
