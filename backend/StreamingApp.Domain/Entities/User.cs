using Microsoft.AspNetCore.Identity;
using StreamingApp.Domain.Enums;

namespace StreamingApp.Domain.Entities;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsAdmin { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
    public ICollection<UserList> UserLists { get; set; } = new List<UserList>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
