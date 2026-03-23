namespace StreamingApp.Application.DTOs.Content;

public record EpisodeDto(
    Guid Id,
    Guid ContentId,
    string Title,
    string Description,
    int SeasonNumber,
    int EpisodeNumber,
    int DurationMinutes,
    string? ThumbnailUrl,
    string? HlsManifestUrl,
    string Status
);
