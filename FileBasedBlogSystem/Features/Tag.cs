namespace FileBlogSystem.Features;

public class Tag
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? ModifiedAt { get; set; }
}
