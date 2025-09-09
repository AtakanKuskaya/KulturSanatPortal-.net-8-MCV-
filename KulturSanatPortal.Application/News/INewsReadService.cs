using KulturSanatPortal.Application.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KulturSanatPortal.Application.News
{
    public interface INewsReadService
    {
        Task<PagedResult<NewsListItemDto>> ListAsync(int page, int pageSize, CancellationToken ct);
        Task<IReadOnlyList<NewsListItemDto>> ListLatestAsync(int take, CancellationToken ct);
        Task<NewsDetailsDto?> GetBySlugAsync(string slug, CancellationToken ct);
    }
}
