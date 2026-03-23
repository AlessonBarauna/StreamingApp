namespace StreamingApp.Domain.Entities;

public class UserList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ContentId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Content? Content { get; set; }
}
