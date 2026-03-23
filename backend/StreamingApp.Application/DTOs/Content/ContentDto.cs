using StreamingApp.Domain.Enums;

namespace StreamingApp.Application.DTOs.Content;

public record ContentDto(
    Guid Id,
    string Title,
    string Description,
    string Type,
    string ThumbnailUrl,
    string? BackdropUrl,
    int ReleaseYear,
    int? DurationMinutes,
    string AgeRating,
    string Status,
    string? HlsManifestUrl,
    string CategoryName,
    Guid CategoryId,
    long ViewCount,
    bool IsFeatured,
    DateTime CreatedAt
);
