using KulturSanatPortal.Application.Categories;
using KulturSanatPortal.Application.Events;
using KulturSanatPortal.Web.Models.Home; // MiniCalendarVm, MiniDay
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KulturSanatPortal.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventReadService _events;
        private readonly ICategoryReadService _categories;

        // const yerine static readonly: Hot Reload sırasında ENC0011 uyarısını engeller
        private static readonly int PageSize = 12;

        public EventsController(IEventReadService events, ICategoryReadService categories)
        {
            _events = events;
            _categories = categories;
        }

        [HttpGet]
        [Route("etkinlikler", Name = "EventsIndex")]
        [Route("events")]
        public async Task<IActionResult> Index(
            int? y, int? m, DateTime? date, int? categoryId, string? week, int page = 1, CancellationToken ct = default)
        {
            var now = DateTime.Today;
            var year = y ?? now.Year;
            var month = m ?? now.Month;

            // 1) Takvim için ay verisi
            var monthItems = await _events.ListForCalendarAsync(year, month, ct);
            var counts = monthItems
                .GroupBy(e => e.StartLocal.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var first = new DateTime(year, month, 1);
            var offset = ((int)first.DayOfWeek + 6) % 7; // Pazartesi başlangıç
            var start = first.AddDays(-offset);

            var days = new List<MiniDay>(42);
            for (int i = 0; i < 42; i++)
            {
                var d = start.AddDays(i);
                counts.TryGetValue(d.Date, out var c);
                days.Add(new MiniDay(d, d.Month == month, d.Date == now.Date, c));
            }
            ViewBag.CalendarModel = new MiniCalendarVm { Year = year, Month = month, Days = days };

            // 2) Kategoriler
            ViewBag.Categories = await _categories.ListAsync(ct);

            // 3) Sağ liste (ilk yükleme)
            IEnumerable<object> list;
            if (date.HasValue)
            {
                list = monthItems
                        .Where(e => e.StartLocal.Date == date.Value.Date)
                        .Cast<object>()
                        .ToList();
            }
            else
            {
                var vm = await _events.ListUpcomingAsync(categoryId, null, year, month, page, PageSize, ct);

                var raw =
                    (vm?.GetType().GetProperty("Items")?.GetValue(vm) as System.Collections.IEnumerable) ??
                    (vm?.GetType().GetProperty("Events")?.GetValue(vm) as System.Collections.IEnumerable) ??
                    (vm as System.Collections.IEnumerable);

                list = raw != null ? raw.Cast<object>().ToList() : Enumerable.Empty<object>();
            }

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.SelectedCategory = categoryId;

            return View(list);
        }

        // GET /Events/ListPartial?date=2025-09-19&categoryId=3
        [HttpGet]
        public async Task<IActionResult> ListPartial(DateTime? date, int? categoryId, CancellationToken ct = default)
        {
            IEnumerable<object> list;

            if (date.HasValue)
            {
                var monthItems = await _events.ListForCalendarAsync(date.Value.Year, date.Value.Month, ct);
                list = monthItems
                        .Where(e => e.StartLocal.Date == date.Value.Date)
                        .Cast<object>()
                        .ToList();
            }
            else
            {
                var vm = await _events.ListUpcomingAsync(categoryId, null, null, null, 1, PageSize, ct);

                var raw =
                    (vm?.GetType().GetProperty("Items")?.GetValue(vm) as System.Collections.IEnumerable) ??
                    (vm?.GetType().GetProperty("Events")?.GetValue(vm) as System.Collections.IEnumerable) ??
                    (vm as System.Collections.IEnumerable);

                list = raw != null ? raw.Cast<object>().ToList() : Enumerable.Empty<object>();
            }

            return PartialView("_EventList", list);
        }
    }
}
