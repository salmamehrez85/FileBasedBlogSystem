using FileBlogSystem.Services;
using FileBlogSystem.Features;

namespace FileBlogSystem.Endpoints;

public static class MediaEndpoints
{
    public static void MapMediaEndpoints(this WebApplication app)
    {
        app.MapPost("/posts/{slug}/upload", async (
            HttpContext context,
            string slug,
            IFormFile file,
            PostService postService,
            MediaService imageService) =>
        {
            var user = context.Items["User"] as User;
            if (user == null) return Results.Unauthorized();

            var post = await postService.GetPostBySlugAsync(slug);
            if (post == null) return Results.NotFound("Post not found.");

            if (file == null || file.Length == 0)
                return Results.BadRequest("No file was uploaded.");

            var postDir = Path.Combine("Content", "Posts", $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}", "assets");
            Directory.CreateDirectory(postDir);
            await imageService.SaveAndResizeImageAsync(file, postDir);

            return Results.Ok("Image uploaded and resized.");
        }).RequireAuthorization().DisableAntiforgery();
    }
} 