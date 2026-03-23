using Microsoft.EntityFrameworkCore;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Enums;
using StreamingApp.Domain.Interfaces;
using StreamingApp.Infrastructure.Data;

namespace StreamingApp.Infrastructure.Repositories;

public class ContentRepository : Repository<Content>, IContentRepository
{
    public ContentRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Content> Items, int Total)> GetPagedAsync(
        int page, int limit,
        Guid? categoryId = null,
        string? search = null,
        string? type = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(c => c.Category)
            .Where(c => c.Status == TranscodingStatus.Ready || c.Status == TranscodingStatus.Draft)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.ToLower().Contains(search.ToLower()));

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<ContentType>(type, true, out var contentType))
            query = query.Where(c => c.Type == contentType);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IEnumerable<Content>> GetFeaturedAsync(CancellationToken ct = default)
        => await _dbSet
            .Include(c => c.Category)
            .Where(c => c.IsFeatured && c.Status == TranscodingStatus.Ready)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync(ct);

    public async Task<IEnumerable<Content>> GetTrendingAsync(int limit = 20, CancellationToken ct = default)
        => await _dbSet
            .Include(c => c.Category)
            .Where(c => c.Status == TranscodingStatus.Ready)
            .OrderByDescending(c => c.ViewCount)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IEnumerable<Content>> GetNewReleasesAsync(int limit = 20, CancellationToken ct = default)
        => await _dbSet
            .Include(c => c.Category)
            .Where(c => c.Status == TranscodingStatus.Ready)
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<Content?> GetWithEpisodesAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Episodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}
