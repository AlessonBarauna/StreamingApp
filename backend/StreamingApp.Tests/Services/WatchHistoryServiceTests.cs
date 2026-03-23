using Moq;
using StreamingApp.Application.DTOs.Stream;
using StreamingApp.Application.Services;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.Tests.Services;

public class WatchHistoryServiceTests
{
    private readonly Mock<IRepository<WatchHistory>> _historyRepoMock = new();
    private readonly Mock<IRepository<UserList>> _userListRepoMock = new();
    private readonly Mock<IRepository<Rating>> _ratingRepoMock = new();
    private readonly WatchHistoryService _sut;

    public WatchHistoryServiceTests()
    {
        _sut = new WatchHistoryService(
            _historyRepoMock.Object,
            _userListRepoMock.Object,
            _ratingRepoMock.Object);
    }

    // --- GetProgressAsync ---

    [Fact]
    public async Task GetProgressAsync_WhenNoHistory_ReturnsZeroProgress()
    {
        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WatchHistory, bool>>>(), default))
            .ReturnsAsync(new List<WatchHistory>());

        var result = await _sut.GetProgressAsync("user-1", Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.ProgressSeconds);
        Assert.Equal(0, result.Value.PercentComplete);
        Assert.False(result.Value.IsCompleted);
    }

    [Fact]
    public async Task GetProgressAsync_WhenHistoryExists_ReturnsCorrectPercent()
    {
        var history = new WatchHistory
        {
            UserId = "user-1",
            ContentId = Guid.NewGuid(),
            ProgressSeconds = 50,
            TotalSeconds = 100
        };

        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WatchHistory, bool>>>(), default))
            .ReturnsAsync(new List<WatchHistory> { history });

        var result = await _sut.GetProgressAsync("user-1", history.ContentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value!.ProgressSeconds);
        Assert.Equal(50.0, result.Value.PercentComplete);
    }

    // --- SaveProgressAsync ---

    [Fact]
    public async Task SaveProgressAsync_WhenNoHistory_CreatesNewEntry()
    {
        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WatchHistory, bool>>>(), default))
            .ReturnsAsync(new List<WatchHistory>());
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<WatchHistory>(), default)).Returns(Task.CompletedTask);
        _historyRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.SaveProgressAsync("user-1", Guid.NewGuid(), new SaveProgressDto(30, 100));

        Assert.True(result.IsSuccess);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<WatchHistory>(), default), Times.Once);
    }

    [Fact]
    public async Task SaveProgressAsync_WhenProgressReaches95Percent_MarksAsCompleted()
    {
        var history = new WatchHistory { UserId = "user-1", ContentId = Guid.NewGuid() };

        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WatchHistory, bool>>>(), default))
            .ReturnsAsync(new List<WatchHistory> { history });
        _historyRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        await _sut.SaveProgressAsync("user-1", history.ContentId, new SaveProgressDto(96, 100));

        Assert.NotNull(history.CompletedAt);
    }

    [Fact]
    public async Task SaveProgressAsync_WhenProgressBelow95Percent_DoesNotMarkAsCompleted()
    {
        var history = new WatchHistory { UserId = "user-1", ContentId = Guid.NewGuid() };

        _historyRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WatchHistory, bool>>>(), default))
            .ReturnsAsync(new List<WatchHistory> { history });
        _historyRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        await _sut.SaveProgressAsync("user-1", history.ContentId, new SaveProgressDto(50, 100));

        Assert.Null(history.CompletedAt);
    }

    // --- AddToWatchlistAsync ---

    [Fact]
    public async Task AddToWatchlistAsync_WhenAlreadyExists_ReturnsSuccessWithoutDuplicate()
    {
        _userListRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserList, bool>>>(), default))
            .ReturnsAsync(new List<UserList> { new() });

        var result = await _sut.AddToWatchlistAsync("user-1", Guid.NewGuid());

        Assert.True(result.IsSuccess);
        _userListRepoMock.Verify(r => r.AddAsync(It.IsAny<UserList>(), default), Times.Never);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WhenNotExists_AddsItem()
    {
        _userListRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserList, bool>>>(), default))
            .ReturnsAsync(new List<UserList>());
        _userListRepoMock.Setup(r => r.AddAsync(It.IsAny<UserList>(), default)).Returns(Task.CompletedTask);
        _userListRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.AddToWatchlistAsync("user-1", Guid.NewGuid());

        Assert.True(result.IsSuccess);
        _userListRepoMock.Verify(r => r.AddAsync(It.IsAny<UserList>(), default), Times.Once);
    }

    // --- RemoveFromWatchlistAsync ---

    [Fact]
    public async Task RemoveFromWatchlistAsync_WhenNotInWatchlist_ReturnsNotFound()
    {
        _userListRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserList, bool>>>(), default))
            .ReturnsAsync(new List<UserList>());

        var result = await _sut.RemoveFromWatchlistAsync("user-1", Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    // --- RateContentAsync ---

    [Fact]
    public async Task RateContentAsync_WhenNoExistingRating_CreatesNewRating()
    {
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Rating, bool>>>(), default))
            .ReturnsAsync(new List<Rating>());
        _ratingRepoMock.Setup(r => r.AddAsync(It.IsAny<Rating>(), default)).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.RateContentAsync("user-1", Guid.NewGuid(), isLiked: true);

        Assert.True(result.IsSuccess);
        _ratingRepoMock.Verify(r => r.AddAsync(It.Is<Rating>(rt => rt.IsLiked == true), default), Times.Once);
    }

    [Fact]
    public async Task RateContentAsync_WhenExistingRating_UpdatesRating()
    {
        var existing = new Rating { UserId = "user-1", ContentId = Guid.NewGuid(), IsLiked = true };

        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Rating, bool>>>(), default))
            .ReturnsAsync(new List<Rating> { existing });
        _ratingRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.RateContentAsync("user-1", existing.ContentId, isLiked: false);

        Assert.True(result.IsSuccess);
        Assert.False(existing.IsLiked);
        _ratingRepoMock.Verify(r => r.Update(existing), Times.Once);
        _ratingRepoMock.Verify(r => r.AddAsync(It.IsAny<Rating>(), default), Times.Never);
    }
}
