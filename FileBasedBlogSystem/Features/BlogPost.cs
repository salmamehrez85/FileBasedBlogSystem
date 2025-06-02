namespace FileBlogSystem.Features;

public class BlogPost
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = "draft"; 
    public string Author { get; set; } = string.Empty;
}
