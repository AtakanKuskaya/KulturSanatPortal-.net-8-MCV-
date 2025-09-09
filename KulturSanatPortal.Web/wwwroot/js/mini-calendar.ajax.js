/*! MiniCalendar AJAX (refreshsiz + badge destek) */
(function () {
    function isModClick(e) { return e.metaKey || e.ctrlKey || e.shiftKey || e.altKey || e.button !== 0; }

    // .calendar-mini içindeki prev/next tıklamalarını yakala
    document.addEventListener('click', function (e) {
        const trg = e.target.closest('a,button');
        if (!trg || isModClick(e)) return;

        const cal = trg.closest('.calendar-mini');
        if (!cal) return;

        // Farklı nav desenlerini destekle
        const nav = trg.getAttribute('data-cal-nav');
        const rel = trg.getAttribute('rel');
        const hasChevron = trg.querySelector('.bi-chevron-left, .bi-chevron-right');
        const text = (trg.textContent || '').trim();

        const looksLikeNav =
            nav === 'prev' || nav === 'next' ||
            rel === 'prev' || rel === 'next' ||
            hasChevron || text === '‹' || text === '›';

        if (!looksLikeNav) return;

        const href = trg.getAttribute('href');
        if (!href) return;

        e.preventDefault();

        fetch(href, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => r.text())
            .then(html => {
                const tmp = document.createElement('div');
                tmp.innerHTML = html;
                // Dönen parçada ilk .calendar-mini’yi bul, yoksa tüm html’i kullan
                const nextCal = tmp.querySelector('.calendar-mini') || tmp.firstElementChild;
                if (nextCal) {
                    cal.replaceWith(nextCal);
                    // badge fallback (data-count>0 olup .badge yoksa minik nokta)
                    enhanceBadges(nextCal);
                } else {
                    window.location.href = href; // güvenli geri dönüş
                }
            })
            .catch(() => window.location.href = href);
    }, { passive: false });

    function enhanceBadges(root) {
        (root.querySelectorAll?.('.day') || []).forEach(day => {
            if (day.querySelector('.badge')) return;
            const cnt = parseInt(day.getAttribute('data-count') || '0', 10);
            if (cnt > 0 && !day.querySelector('.badge-fallback')) {
                const dot = document.createElement('span');
                dot.className = 'badge-fallback';
                dot.setAttribute('aria-hidden', 'true');
                day.appendChild(dot);
            }
        });
    }

    // İlk yüklemede de fallback noktaları garanti et
    document.addEventListener('DOMContentLoaded', () => enhanceBadges(document));
})();
