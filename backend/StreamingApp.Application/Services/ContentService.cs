using StreamingApp.Application.DTOs.Content;
using StreamingApp.Domain.Common;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.Application.Services;

public class ContentService
{
    private readonly IContentRepository _contentRepo;
    private readonly IRepository<Episode> _episodeRepo;

    public ContentService(IContentRepository contentRepo, IRepository<Episode> episodeRepo)
    {
        _contentRepo = contentRepo;
        _episodeRepo = episodeRepo;
    }

    public async Task<Result<PagedResultDto<ContentDto>>> GetPagedAsync(int page, int limit, Guid? categoryId, string? search, string? type, CancellationToken ct = default)
    {
        var (items, total) = await _contentRepo.GetPagedAsync(page, limit, categoryId, search, type, ct);
        return Result<PagedResultDto<ContentDto>>.Success(new PagedResultDto<ContentDto>(items.Select(MapToDto), total, page, limit));
    }

    public async Task<Result<IEnumerable<ContentDto>>> GetFeaturedAsync(CancellationToken ct = default)
    {
        var items = await _contentRepo.GetFeaturedAsync(ct);
        return Result<IEnumerable<ContentDto>>.Success(items.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<ContentDto>>> GetTrendingAsync(CancellationToken ct = default)
    {
        var items = await _contentRepo.GetTrendingAsync(20, ct);
        return Result<IEnumerable<ContentDto>>.Success(items.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<ContentDto>>> GetNewReleasesAsync(CancellationToken ct = default)
    {
        var items = await _contentRepo.GetNewReleasesAsync(20, ct);
        return Result<IEnumerable<ContentDto>>.Success(items.Select(MapToDto));
    }

    public async Task<Result<ContentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var content = await _contentRepo.GetWithEpisodesAsync(id, ct);
        if (content == null) return Result<ContentDto>.NotFound();
        return Result<ContentDto>.Success(MapToDto(content));
    }

    public async Task<Result<IEnumerable<EpisodeDto>>> GetEpisodesAsync(Guid id, CancellationToken ct = default)
    {
        var content = await _contentRepo.GetWithEpisodesAsync(id, ct);
        if (content == null) return Result<IEnumerable<EpisodeDto>>.NotFound();
        return Result<IEnumerable<EpisodeDto>>.Success(content.Episodes.Select(MapEpisodeToDto));
    }

    public async Task<Result<ContentDto>> CreateAsync(CreateContentDto dto, CancellationToken ct = default)
    {
        if (!Enum.TryParse<Domain.Enums.ContentType>(dto.Type, true, out var contentType))
            return Result<ContentDto>.Failure("Tipo de conteúdo inválido.");

        var content = new Content
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = contentType,
            ReleaseYear = dto.ReleaseYear,
            DurationMinutes = dto.DurationMinutes,
            AgeRating = dto.AgeRating,
            CategoryId = dto.CategoryId,
            IsFeatured = dto.IsFeatured,
            ThumbnailUrl = string.Empty
        };

        await _contentRepo.AddAsync(content, ct);
        await _contentRepo.SaveChangesAsync(ct);
        return Result<ContentDto>.Success(MapToDto(content));
    }

    public async Task<Result<ContentDto>> UpdateAsync(Guid id, CreateContentDto dto, CancellationToken ct = default)
    {
        var content = await _contentRepo.GetByIdAsync(id, ct);
        if (content == null) return Result<ContentDto>.NotFound();

        if (!Enum.TryParse<Domain.Enums.ContentType>(dto.Type, true, out var contentType))
            return Result<ContentDto>.Failure("Tipo de conteúdo inválido.");

        content.Title = dto.Title;
        content.Description = dto.Description;
        content.Type = contentType;
        content.ReleaseYear = dto.ReleaseYear;
        content.DurationMinutes = dto.DurationMinutes;
        content.AgeRating = dto.AgeRating;
        content.CategoryId = dto.CategoryId;
        content.IsFeatured = dto.IsFeatured;

        _contentRepo.Update(content);
        await _contentRepo.SaveChangesAsync(ct);
        return Result<ContentDto>.Success(MapToDto(content));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var content = await _contentRepo.GetByIdAsync(id, ct);
        if (content == null) return Result.NotFound();

        _contentRepo.Remove(content);
        await _contentRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<EpisodeDto>> CreateEpisodeAsync(Guid contentId, CreateEpisodeDto dto, CancellationToken ct = default)
    {
        var content = await _contentRepo.GetByIdAsync(contentId, ct);
        if (content is null) return Result<EpisodeDto>.NotFound();

        var duplicate = await _episodeRepo.FindAsync(
            e => e.ContentId == contentId && e.SeasonNumber == dto.SeasonNumber && e.EpisodeNumber == dto.EpisodeNumber, ct);
        if (duplicate.Any())
            return Result<EpisodeDto>.Failure($"Episódio S{dto.SeasonNumber:D2}E{dto.EpisodeNumber:D2} já existe para esse conteúdo.");

        var episode = new Episode
        {
            ContentId = contentId,
            Title = dto.Title,
            Description = dto.Description,
            SeasonNumber = dto.SeasonNumber,
            EpisodeNumber = dto.EpisodeNumber,
            DurationMinutes = dto.DurationMinutes
        };

        await _episodeRepo.AddAsync(episode, ct);
        await _episodeRepo.SaveChangesAsync(ct);
        return Result<EpisodeDto>.Success(MapEpisodeToDto(episode));
    }

    public async Task<Result<EpisodeDto>> UpdateEpisodeAsync(Guid contentId, Guid episodeId, UpdateEpisodeDto dto, CancellationToken ct = default)
    {
        var episode = await _episodeRepo.GetByIdAsync(episodeId, ct);
        if (episode is null || episode.ContentId != contentId) return Result<EpisodeDto>.NotFound();

        episode.Title = dto.Title;
        episode.Description = dto.Description;
        episode.DurationMinutes = dto.DurationMinutes;

        _episodeRepo.Update(episode);
        await _episodeRepo.SaveChangesAsync(ct);
        return Result<EpisodeDto>.Success(MapEpisodeToDto(episode));
    }

    public async Task<Result> DeleteEpisodeAsync(Guid contentId, Guid episodeId, CancellationToken ct = default)
    {
        var episode = await _episodeRepo.GetByIdAsync(episodeId, ct);
        if (episode is null || episode.ContentId != contentId) return Result.NotFound();

        _episodeRepo.Remove(episode);
        await _episodeRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static ContentDto MapToDto(Content c) => new(
        c.Id, c.Title, c.Description,
        c.Type.ToString(), c.ThumbnailUrl, c.BackdropUrl,
        c.ReleaseYear, c.DurationMinutes, c.AgeRating,
        c.Status.ToString(), c.HlsManifestUrl,
        c.Category?.Name ?? string.Empty, c.CategoryId,
        c.ViewCount, c.IsFeatured, c.CreatedAt
    );

    private static EpisodeDto MapEpisodeToDto(Episode e) => new(
        e.Id, e.ContentId, e.Title, e.Description,
        e.SeasonNumber, e.EpisodeNumber, e.DurationMinutes,
        e.ThumbnailUrl, e.HlsManifestUrl, e.Status.ToString()
    );
}
