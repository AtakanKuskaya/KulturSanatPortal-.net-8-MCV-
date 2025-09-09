
/*! MiniCalendar Final (refreshless + badge fallback)
 * - Intercepts prev/next in .calendar-mini:
 *    a[data-cal-nav], button[data-cal-nav],
 *    a[rel=prev|next], .btn with .bi-chevron-left/right
 * - Fetches partial via AJAX and swaps only the calendar block
 * - Fallback: full navigation if AJAX fails
 * - Badge visibility fallback: if no .badge but data-count>0 -> draw small dot
 */
(function () {
    function isModClick(e) { return e.metaKey || e.ctrlKey || e.shiftKey || e.altKey || e.button !== 0; }

    function enhanceBadges(root) {
        const scope = root?.querySelector?.('.calendar-mini') || root;
        if (!scope) return;
        scope.querySelectorAll('.day').forEach(day => {
            // If real badge exists, skip
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

    function swapCalendar(oldCal, html) {
        const temp = document.createElement('div');
        temp.innerHTML = html;

        // Prefer id match first
        const id = oldCal.id ? ('#' + CSS.escape(oldCal.id)) : null;
        let nextCal = id ? temp.querySelector(id) : null;
        if (!nextCal) nextCal = temp.querySelector('.calendar-mini');
        if (!nextCal) nextCal = temp.firstElementChild;

        if (nextCal) {
            oldCal.replaceWith(nextCal);
            enhanceBadges(nextCal);
            // fire custom event
            const ev = new CustomEvent('miniCalendar:swapped', { bubbles: true });
            nextCal.dispatchEvent(ev);
            return true;
        }
        return false;
    }

    function handleNav(e) {
        if (isModClick(e)) return;

        const trg = e.target.closest('a,button');
        if (!trg) return;
        const cal = trg.closest('.calendar-mini');
        if (!cal) return;

        // Match different nav patterns
        const hasData = trg.hasAttribute('data-cal-nav');
        const hasChevron = trg.querySelector('.bi-chevron-left, .bi-chevron-right');
        const isRelPrevNext = (trg.getAttribute('rel') === 'prev' || trg.getAttribute('rel') === 'next');

        if (!hasData && !hasChevron && !isRelPrevNext) return;

        const href = trg.getAttribute('href');
        if (!href) return;

        e.preventDefault();

        fetch(href, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => r.text())
            .then(html => {
                if (!swapCalendar(cal, html)) {
                    // If parsing fails, follow the link
                    window.location.href = href;
                }
            })
            .catch(() => { window.location.href = href; });
    }

    document.addEventListener('click', handleNav, { passive: false });

    document.addEventListener('DOMContentLoaded', function () {
        enhanceBadges(document);
    });

    // Re-apply badge enhancement when nodes are added dynamically
    const mo = new MutationObserver(muts => {
        for (const m of muts) {
            for (const n of m.addedNodes) {
                if (n.nodeType === 1 && (n.matches?.('.calendar-mini') || n.querySelector?.('.calendar-mini'))) {
                    enhanceBadges(n);
                }
            }
        }
    });
    mo.observe(document.documentElement, { childList: true, subtree: true });
})();
