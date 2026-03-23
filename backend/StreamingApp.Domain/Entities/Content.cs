using StreamingApp.Domain.Enums;

namespace StreamingApp.Domain.Entities;

public class Content
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? BackdropUrl { get; set; }
    public int ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string AgeRating { get; set; } = "L";
    public TranscodingStatus Status { get; set; } = TranscodingStatus.Draft;
    public string? HlsManifestUrl { get; set; }
    public string? OriginalFileKey { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long ViewCount { get; set; }
    public bool IsFeatured { get; set; }

    public Category? Category { get; set; }
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
    public ICollection<UserList> UserLists { get; set; } = new List<UserList>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
