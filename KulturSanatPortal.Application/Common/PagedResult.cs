using System;
using System.Collections.Generic;

namespace KulturSanatPortal.Application.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }        // 1-based
        public int PageSize { get; init; }
        public int TotalCount { get; init; }

        // Hesaplanan yardımcılar
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        // Parametresiz kurucu (object initializer için)
        public PagedResult() { }

        // GERİYE DÖNÜK UYUMLU kurucu (servislerinizde kullanılan)
        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items ?? Array.Empty<T>();
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
