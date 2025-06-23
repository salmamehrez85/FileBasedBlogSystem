using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
namespace FileBlogSystem.Features;

public class BlogPost
{
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PostStatus Status { get; set; } = PostStatus.Draft;
    [MaxLength(50)]
    public string Author { get; set; } = string.Empty;
}

