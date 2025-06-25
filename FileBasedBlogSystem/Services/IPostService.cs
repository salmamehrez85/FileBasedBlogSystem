using FileBlogSystem.Features;

namespace FileBlogSystem.Services;

public interface IPostService
{
    Task<IEnumerable<BlogPost>> GetAllPostsAsync();
    Task<BlogPost?> GetPostBySlugAsync(string slug);
    Task SavePostAsync(BlogPost post);
    Task DeletePostAsync(string slug);
} 