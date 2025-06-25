using FileBlogSystem.Features;
using System.Text.Json;

namespace FileBlogSystem.Services;

public class PostService : IPostService
{
    private readonly string _postsRoot = Path.Combine("Content", "Posts");

    public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
    {
        if (!Directory.Exists(_postsRoot))
            return Enumerable.Empty<BlogPost>();

        var posts = new List<BlogPost>();

        foreach (var dir in Directory.GetDirectories(_postsRoot))
        {
            var metaPath = Path.Combine(dir, "meta.json");
            if (File.Exists(metaPath))
            {
                var json = await File.ReadAllTextAsync(metaPath);
                var post = JsonSerializer.Deserialize<BlogPost>(json);
                if (post != null)
                    posts.Add(post);
            }
        }

        return posts.OrderByDescending(p => p.PublishedDate);
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        var postDir = Directory.GetDirectories(_postsRoot)
            .FirstOrDefault(d => d.EndsWith(slug));

        if (postDir == null) return null;

        var metaPath = Path.Combine(postDir, "meta.json");
        if (!File.Exists(metaPath)) return null;

        var json = await File.ReadAllTextAsync(metaPath);
        return JsonSerializer.Deserialize<BlogPost>(json);
    }

    public async Task SavePostAsync(BlogPost post)
    {
        var dirName = $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}";
        var postDir = Path.Combine(_postsRoot, dirName);

        Directory.CreateDirectory(postDir);

        var metaPath = Path.Combine(postDir, "meta.json");
        var contentPath = Path.Combine(postDir, "content.md");

        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(post, new JsonSerializerOptions { WriteIndented = true }));
        await File.WriteAllTextAsync(contentPath, post.Body);
    }

    public async Task DeletePostAsync(string slug)
    {
        var post = await GetPostBySlugAsync(slug);
        if (post == null) return;

        var dirName = $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}";
        var postDir = Path.Combine(_postsRoot, dirName);

        if (Directory.Exists(postDir))
        {
            Directory.Delete(postDir, true);
        }
    }
}
