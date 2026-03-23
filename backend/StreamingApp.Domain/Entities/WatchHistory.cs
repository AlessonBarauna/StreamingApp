namespace StreamingApp.Domain.Entities;

public class WatchHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ContentId { get; set; }
    public Guid? EpisodeId { get; set; }
    public int ProgressSeconds { get; set; }
    public int TotalSeconds { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Content? Content { get; set; }
}
