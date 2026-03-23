namespace StreamingApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;

    public ICollection<Content> Contents { get; set; } = new List<Content>();
}
