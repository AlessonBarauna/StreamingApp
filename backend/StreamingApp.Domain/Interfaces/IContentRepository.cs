using StreamingApp.Domain.Entities;

namespace StreamingApp.Domain.Interfaces;

public interface IContentRepository : IRepository<Content>
{
    Task<(IEnumerable<Content> Items, int Total)> GetPagedAsync(
        int page, int limit,
        Guid? categoryId = null,
        string? search = null,
        string? type = null,
        CancellationToken ct = default);
    Task<IEnumerable<Content>> GetFeaturedAsync(CancellationToken ct = default);
    Task<IEnumerable<Content>> GetTrendingAsync(int limit = 20, CancellationToken ct = default);
    Task<IEnumerable<Content>> GetNewReleasesAsync(int limit = 20, CancellationToken ct = default);
    Task<Content?> GetWithEpisodesAsync(Guid id, CancellationToken ct = default);
}
