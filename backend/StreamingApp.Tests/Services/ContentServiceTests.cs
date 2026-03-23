using Moq;
using StreamingApp.Application.DTOs.Content;
using StreamingApp.Application.Services;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Enums;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.Tests.Services;

public class ContentServiceTests
{
    private readonly Mock<IContentRepository> _contentRepoMock = new();
    private readonly Mock<IRepository<Episode>> _episodeRepoMock = new();
    private readonly ContentService _sut;

    public ContentServiceTests()
    {
        _sut = new ContentService(_contentRepoMock.Object, _episodeRepoMock.Object);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenContentExists_ReturnsSuccess()
    {
        var content = MakeContent();
        _contentRepoMock.Setup(r => r.GetWithEpisodesAsync(content.Id, default))
            .ReturnsAsync(content);

        var result = await _sut.GetByIdAsync(content.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(content.Id, result.Value!.Id);
        Assert.Equal(content.Title, result.Value.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContentNotFound_ReturnsNotFound()
    {
        _contentRepoMock.Setup(r => r.GetWithEpisodesAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Content?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WithValidDto_SavesAndReturnsContent()
    {
        var dto = new CreateContentDto("Filme Teste", "Descrição", "Movie", 2024, 120, "L", Guid.NewGuid(), false);

        _contentRepoMock.Setup(r => r.AddAsync(It.IsAny<Content>(), default)).Returns(Task.CompletedTask);
        _contentRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Filme Teste", result.Value!.Title);
        _contentRepoMock.Verify(r => r.AddAsync(It.IsAny<Content>(), default), Times.Once);
        _contentRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidType_ReturnsFailure()
    {
        var dto = new CreateContentDto("Título", "Desc", "TipoInvalido", 2024, null, "L", Guid.NewGuid(), false);

        var result = await _sut.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        _contentRepoMock.Verify(r => r.AddAsync(It.IsAny<Content>(), default), Times.Never);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenContentExists_RemovesAndReturnsSuccess()
    {
        var content = MakeContent();
        _contentRepoMock.Setup(r => r.GetByIdAsync(content.Id, default)).ReturnsAsync(content);
        _contentRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(content.Id);

        Assert.True(result.IsSuccess);
        _contentRepoMock.Verify(r => r.Remove(content), Times.Once);
        _contentRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenContentNotFound_ReturnsNotFound()
    {
        _contentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Content?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        _contentRepoMock.Verify(r => r.Remove(It.IsAny<Content>()), Times.Never);
    }

    // --- CreateEpisodeAsync ---

    [Fact]
    public async Task CreateEpisodeAsync_WhenContentNotFound_ReturnsNotFound()
    {
        _contentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Content?)null);

        var result = await _sut.CreateEpisodeAsync(Guid.NewGuid(), new CreateEpisodeDto("Ep 1", "", 1, 1, 45));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateEpisodeAsync_WhenDuplicateEpisode_ReturnsFailure()
    {
        var content = MakeContent();
        var existingEpisode = new Episode { ContentId = content.Id, SeasonNumber = 1, EpisodeNumber = 1 };

        _contentRepoMock.Setup(r => r.GetByIdAsync(content.Id, default)).ReturnsAsync(content);
        _episodeRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Episode, bool>>>(), default))
            .ReturnsAsync(new List<Episode> { existingEpisode });

        var result = await _sut.CreateEpisodeAsync(content.Id, new CreateEpisodeDto("Ep Dup", "", 1, 1, 45));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        _episodeRepoMock.Verify(r => r.AddAsync(It.IsAny<Episode>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateEpisodeAsync_WithValidData_SavesAndReturnsEpisode()
    {
        var content = MakeContent();
        _contentRepoMock.Setup(r => r.GetByIdAsync(content.Id, default)).ReturnsAsync(content);
        _episodeRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Episode, bool>>>(), default))
            .ReturnsAsync(new List<Episode>());
        _episodeRepoMock.Setup(r => r.AddAsync(It.IsAny<Episode>(), default)).Returns(Task.CompletedTask);
        _episodeRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.CreateEpisodeAsync(content.Id, new CreateEpisodeDto("Episódio 1", "Desc", 1, 1, 45));

        Assert.True(result.IsSuccess);
        Assert.Equal("Episódio 1", result.Value!.Title);
        Assert.Equal(1, result.Value.SeasonNumber);
        Assert.Equal(1, result.Value.EpisodeNumber);
    }

    // --- Helpers ---

    private static Content MakeContent() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Conteúdo Teste",
        Description = "Desc",
        Type = ContentType.Movie,
        CategoryId = Guid.NewGuid(),
        Status = TranscodingStatus.Ready
    };
}
