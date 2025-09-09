/* ============== Events UI helpers (filter UX + badge label) ============== */
(function () {
    // Filtre formu için otomatik gönderme (form[data-autosubmit])
    document.addEventListener('change', function (e) {
        const f = e.target.closest('form[data-autosubmit]');
        if (!f) return;
        const btn = f.querySelector('[type=submit]');
        if (btn) btn.disabled = true;
        f.submit();
    });

    // Kartlardaki tarih rozetini [data-dt="yyyy-mm-dd"] üzerinden doldur
    const TR_MONTHS = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
    function hydrateBadges(root) {
        (root.querySelectorAll?.('.events-page .evt-date-badge[data-dt]') || []).forEach(b => {
            const s = b.getAttribute('data-dt'); // ISO yyyy-mm-dd
            if (!s) return;
            const d = new Date(s);
            if (Number.isNaN(+d)) return;
            const m = TR_MONTHS[d.getMonth()] || "";
            const dd = String(d.getDate());
            const mEl = b.querySelector('.m'); const dEl = b.querySelector('.d');
            if (mEl) mEl.textContent = m;
            if (dEl) dEl.textContent = dd;
        });
    }

    document.addEventListener('DOMContentLoaded', () => hydrateBadges(document));

    // AJAX ile içerik değişirse tekrar işle
    const mo = new MutationObserver(muts => {
        for (const m of muts) {
            for (const n of m.addedNodes) {
                if (n.nodeType === 1) hydrateBadges(n);
            }
        }
    });
    mo.observe(document.documentElement, { childList: true, subtree: true });
})();
