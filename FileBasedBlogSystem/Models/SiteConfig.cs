namespace FileBlogSystem.Models;

public class SiteConfig
{
    public string SiteTitle { get; set; } = "My Blog";
    public string SiteDescription { get; set; } = "A simple file-based blog system.";
    public string BaseUrl { get; set; } = "https://localhost:5030";
    public string AdminEmail { get; set; } = string.Empty;
}
