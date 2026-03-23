using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StreamingApp.Domain.Entities;

namespace StreamingApp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Content> Contents { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Episode> Episodes { get; set; } = null!;
    public DbSet<WatchHistory> WatchHistories { get; set; } = null!;
    public DbSet<UserList> UserLists { get; set; } = null!;
    public DbSet<Rating> Ratings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Content>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.HasOne(x => x.Category).WithMany(c => c.Contents).HasForeignKey(x => x.CategoryId);
            e.HasIndex(x => x.Title);
            e.HasIndex(x => x.IsFeatured);
            e.HasIndex(x => x.Status);
        });

        builder.Entity<Episode>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Content).WithMany(c => c.Episodes).HasForeignKey(x => x.ContentId);
            e.HasIndex(x => new { x.ContentId, x.SeasonNumber, x.EpisodeNumber }).IsUnique();
        });

        builder.Entity<WatchHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.WatchHistories).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Content).WithMany(c => c.WatchHistories).HasForeignKey(x => x.ContentId);
            e.HasIndex(x => new { x.UserId, x.ContentId });
        });

        builder.Entity<UserList>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.UserLists).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Content).WithMany(c => c.UserLists).HasForeignKey(x => x.ContentId);
            e.HasIndex(x => new { x.UserId, x.ContentId }).IsUnique();
        });

        builder.Entity<Rating>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.Ratings).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Content).WithMany(c => c.Ratings).HasForeignKey(x => x.ContentId);
            e.HasIndex(x => new { x.UserId, x.ContentId }).IsUnique();
        });

        builder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Slug).IsUnique();
        });
    }
}
