using FileBlogSystem.Models;
using System.Text.Json;

namespace FileBlogSystem.Services;

public class PostService
{
    private readonly string _postsRoot = Path.Combine("Content", "Posts");

    public IEnumerable<BlogPost> GetAllPosts()
    {
        if (!Directory.Exists(_postsRoot))
            return Enumerable.Empty<BlogPost>();

        var posts = new List<BlogPost>();

        foreach (var dir in Directory.GetDirectories(_postsRoot))
        {
            var metaPath = Path.Combine(dir, "meta.json");
            if (File.Exists(metaPath))
            {
                var json = File.ReadAllText(metaPath);
                var post = JsonSerializer.Deserialize<BlogPost>(json);
                if (post != null)
                    posts.Add(post);
            }
        }

        return posts.OrderByDescending(p => p.PublishedDate);
    }

    public BlogPost? GetPostBySlug(string slug)
    {
        var postDir = Directory.GetDirectories(_postsRoot)
            .FirstOrDefault(d => d.EndsWith(slug));

        if (postDir == null) return null;

        var metaPath = Path.Combine(postDir, "meta.json");
        if (!File.Exists(metaPath)) return null;

        var json = File.ReadAllText(metaPath);
        return JsonSerializer.Deserialize<BlogPost>(json);
    }

    public void SavePost(BlogPost post)
    {
        var dirName = $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}";
        var postDir = Path.Combine(_postsRoot, dirName);

        Directory.CreateDirectory(postDir);

        var metaPath = Path.Combine(postDir, "meta.json");
        var contentPath = Path.Combine(postDir, "content.md");

        File.WriteAllText(metaPath, JsonSerializer.Serialize(post, new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(contentPath, post.Body);
    }
}
