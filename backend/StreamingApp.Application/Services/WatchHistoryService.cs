using StreamingApp.Application.DTOs.Stream;
using StreamingApp.Domain.Common;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.Application.Services;

public class WatchHistoryService
{
    private readonly IRepository<WatchHistory> _historyRepo;
    private readonly IRepository<UserList> _userListRepo;
    private readonly IRepository<Rating> _ratingRepo;

    public WatchHistoryService(
        IRepository<WatchHistory> historyRepo,
        IRepository<UserList> userListRepo,
        IRepository<Rating> ratingRepo)
    {
        _historyRepo = historyRepo;
        _userListRepo = userListRepo;
        _ratingRepo = ratingRepo;
    }

    public async Task<Result<ProgressResponseDto>> GetProgressAsync(string userId, Guid contentId, CancellationToken ct = default)
    {
        var history = (await _historyRepo.FindAsync(h => h.UserId == userId && h.ContentId == contentId, ct)).FirstOrDefault();
        if (history == null)
            return Result<ProgressResponseDto>.Success(new ProgressResponseDto(0, 0, 0, false));

        var percent = history.TotalSeconds > 0 ? (double)history.ProgressSeconds / history.TotalSeconds * 100 : 0;
        return Result<ProgressResponseDto>.Success(new ProgressResponseDto(
            history.ProgressSeconds,
            history.TotalSeconds,
            Math.Round(percent, 1),
            history.CompletedAt.HasValue
        ));
    }

    public async Task<Result> SaveProgressAsync(string userId, Guid contentId, SaveProgressDto dto, CancellationToken ct = default)
    {
        var history = (await _historyRepo.FindAsync(h => h.UserId == userId && h.ContentId == contentId, ct)).FirstOrDefault();

        if (history == null)
        {
            history = new WatchHistory { UserId = userId, ContentId = contentId };
            await _historyRepo.AddAsync(history, ct);
        }

        history.ProgressSeconds = dto.Seconds;
        history.TotalSeconds = dto.Total;
        history.UpdatedAt = DateTime.UtcNow;

        var percentComplete = dto.Total > 0 ? (double)dto.Seconds / dto.Total * 100 : 0;
        if (percentComplete >= 95) history.CompletedAt = DateTime.UtcNow;

        _historyRepo.Update(history);
        await _historyRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<WatchHistory>>> GetHistoryAsync(string userId, CancellationToken ct = default)
    {
        var history = await _historyRepo.FindAsync(h => h.UserId == userId, ct);
        return Result<IEnumerable<WatchHistory>>.Success(history.OrderByDescending(h => h.UpdatedAt).Take(50));
    }

    public async Task<Result> AddToWatchlistAsync(string userId, Guid contentId, CancellationToken ct = default)
    {
        var exists = (await _userListRepo.FindAsync(u => u.UserId == userId && u.ContentId == contentId, ct)).Any();
        if (exists) return Result.Success();

        await _userListRepo.AddAsync(new UserList { UserId = userId, ContentId = contentId }, ct);
        await _userListRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveFromWatchlistAsync(string userId, Guid contentId, CancellationToken ct = default)
    {
        var item = (await _userListRepo.FindAsync(u => u.UserId == userId && u.ContentId == contentId, ct)).FirstOrDefault();
        if (item == null) return Result.NotFound();

        _userListRepo.Remove(item);
        await _userListRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<UserList>>> GetWatchlistAsync(string userId, CancellationToken ct = default)
    {
        var list = await _userListRepo.FindAsync(u => u.UserId == userId, ct);
        return Result<IEnumerable<UserList>>.Success(list.OrderByDescending(l => l.AddedAt));
    }

    public async Task<Result> RateContentAsync(string userId, Guid contentId, bool isLiked, CancellationToken ct = default)
    {
        var existing = (await _ratingRepo.FindAsync(r => r.UserId == userId && r.ContentId == contentId, ct)).FirstOrDefault();

        if (existing == null)
        {
            await _ratingRepo.AddAsync(new Rating { UserId = userId, ContentId = contentId, IsLiked = isLiked }, ct);
        }
        else
        {
            existing.IsLiked = isLiked;
            _ratingRepo.Update(existing);
        }

        await _ratingRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
