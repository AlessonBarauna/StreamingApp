using StreamingApp.Domain.Enums;

namespace StreamingApp.Domain.Entities;

public class Episode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ContentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public int DurationMinutes { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? HlsManifestUrl { get; set; }
    public string? OriginalFileKey { get; set; }
    public TranscodingStatus Status { get; set; } = TranscodingStatus.Draft;

    public Content? Content { get; set; }
}
