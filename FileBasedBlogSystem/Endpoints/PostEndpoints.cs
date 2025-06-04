using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FileBlogSystem.Services;
using FileBlogSystem.Features;
namespace FileBlogSystem.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        app.MapGet("/posts", (int? page, int? pageSize, [FromServices] PostService postService) =>
        {
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 10;

            var posts = postService.GetAllPosts()
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.PublishedDate)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize);
            return Results.Ok(posts);
        });

        app.MapGet("/posts/{slug}", (string slug, [FromServices] PostService postService) =>
        {
            var post = postService.GetPostBySlug(slug);
            return post is not null ? Results.Ok(post) : Results.NotFound();
        });

        app.MapGet("/posts/category/{category}", (string category, [FromServices] PostService postService) =>
        {
            var posts = postService.GetAllPosts()
                .Where(p => p.Status == "published" && p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PublishedDate);
            return Results.Ok(posts);
        });

        app.MapGet("/posts/tag/{tag}", (string tag, [FromServices] PostService postService) =>
        {
            var posts = postService.GetAllPosts()
                .Where(p => p.Status == "published" && p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PublishedDate);
            return Results.Ok(posts);
        });

        app.MapGet("/posts/search", (string q, [FromServices] PostService postService) =>
        {
            var posts = postService.GetAllPosts()
                .Where(p => p.Status == "published" &&
                    (p.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                     p.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                     p.Body.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.PublishedDate);
            return Results.Ok(posts);
        });

        app.MapPost("/posts", [Authorize(Roles = "Author,Admin")] (BlogPost post, HttpContext context, [FromServices] PostService postService, [FromServices] RssService rssService) =>
        {
            var user = context.Items["User"] as User;
            if (user == null) return Results.Unauthorized();

            if (!SlugService.IsValidSlug(post.Slug))
                return Results.BadRequest("Slug must be in kebab-case (lowercase, hyphen-separated).");

            var existing = postService.GetPostBySlug(post.Slug);
            if (existing != null)
                return Results.Conflict("Slug already exists.");

            post.Author = user.Username;
            post.PublishedDate = DateTime.UtcNow;
            post.ModifiedDate = DateTime.UtcNow;

            postService.SavePost(post);

            if (post.Status == "published")
                rssService.GenerateRssFeed();

            return Results.Created($"/api/posts/{post.Slug}", post);
        });

        app.MapPut("/posts/{slug}", [Authorize(Roles = "Author,Editor,Admin")] (string slug, BlogPost updatedPost, HttpContext context, [FromServices] PostService postService, [FromServices] RssService rssService) =>
        {
            var user = context.Items["User"] as User;
            if (user == null) return Results.Unauthorized();

            var existing = postService.GetPostBySlug(slug);
            if (existing == null) return Results.NotFound();

            var isOwner = existing.Author == user.Username;
            var isEditor = user.Roles.Contains("Editor") || user.Roles.Contains("Admin");

            if (!isOwner && !isEditor)
                return Results.Forbid();

            if (updatedPost.Slug != slug)
            {
                if (!SlugService.IsValidSlug(updatedPost.Slug))
                    return Results.BadRequest("Slug must be in kebab-case (lowercase, hyphen-separated).");

                var slugExists = postService.GetPostBySlug(updatedPost.Slug);
                if (slugExists != null)
                    return Results.Conflict("Slug already exists.");
            }

            updatedPost.Author = existing.Author;
            updatedPost.PublishedDate = existing.PublishedDate;
            updatedPost.ModifiedDate = DateTime.UtcNow;

            postService.SavePost(updatedPost);

            if (updatedPost.Status == "published")
            {
                rssService.GenerateRssFeed();
            }
            return Results.Ok(updatedPost);
        });

        app.MapDelete("/posts/{slug}", [Authorize(Roles = "Admin")] (string slug, [FromServices] PostService postService) =>
        {
            var post = postService.GetPostBySlug(slug);
            if (post == null) return Results.NotFound();

            postService.DeletePost(slug);
            return Results.NoContent();
        });
    }
} 