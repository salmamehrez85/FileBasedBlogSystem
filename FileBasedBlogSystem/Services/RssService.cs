using System.Text;
using System.Xml;
using FileBlogSystem.Features;

namespace FileBlogSystem.Services;

public class RssService : IRssService
{
    private readonly IPostService _postService;
    private readonly IWebHostEnvironment _env;

    public RssService(IPostService postService, IWebHostEnvironment env)
    {
        _postService = postService;
        _env = env;
    }

    public async Task GenerateRssFeedAsync()
    {
        var posts = (await _postService.GetAllPostsAsync())
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedDate)
            .Take(5);

        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var rssPath = Path.Combine(webRootPath, "feeds");
        Directory.CreateDirectory(rssPath);

        var filePath = Path.Combine(rssPath, "rss.xml");

        var settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };
        using var writer = XmlWriter.Create(filePath, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("rss");
        writer.WriteAttributeString("version", "2.0");

        writer.WriteStartElement("channel");
        writer.WriteElementString("title", "FileBlogSystem RSS Feed");
        writer.WriteElementString("link", "https://localhost:5030");
        writer.WriteElementString("description", "Latest blog posts");

        foreach (var post in posts)
        {
            writer.WriteStartElement("item");
            writer.WriteElementString("title", post.Title);
            writer.WriteElementString("link", $"https://localhost:5030/posts/{post.Slug}");
            writer.WriteElementString("description", post.Description);
            writer.WriteElementString("pubDate", post.PublishedDate.ToString("R"));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}
