namespace StreamingApp.Domain.Entities;

public class Rating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ContentId { get; set; }
    public bool IsLiked { get; set; }

    public User? User { get; set; }
    public Content? Content { get; set; }
}
