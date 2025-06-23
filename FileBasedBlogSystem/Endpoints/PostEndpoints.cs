using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FileBlogSystem.Services;
using FileBlogSystem.Features;

namespace FileBlogSystem.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        app.MapGet("/posts", async (int? page, int? pageSize, [FromServices] PostService postService) =>
        {
            try
            {
                var currentPage = page ?? 1;
                var currentPageSize = pageSize ?? 10;

                var allPosts = (await postService.GetAllPostsAsync())
                    .Where(p => p.Status == PostStatus.Published)
                    .OrderByDescending(p => p.PublishedDate);

                var totalPosts = allPosts.Count();
                var totalPages = (int)Math.Ceiling((double)totalPosts / currentPageSize);

                var posts = allPosts
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToList();

                return Results.Ok(new
                {
                    Posts = posts,
                    CurrentPage = currentPage,
                    PageSize = currentPageSize,
                    TotalPosts = totalPosts,
                    TotalPages = totalPages,
                    HasNextPage = currentPage < totalPages,
                    HasPreviousPage = currentPage > 1
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapGet("/posts/{slug}", async (string slug, [FromServices] PostService postService) =>
        {
            try
            {
                var post = await postService.GetPostBySlugAsync(slug);
                return post is not null ? Results.Ok(post) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapGet("/posts/category/{category}", async (string category, [FromServices] PostService postService) =>
        {
            try
            {
                var posts = (await postService.GetAllPostsAsync())
                    .Where(p => p.Status == PostStatus.Published && p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.PublishedDate)
                    .ToList();
                return Results.Ok(posts);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapGet("/posts/tag/{tag}", async (string tag, [FromServices] PostService postService) =>
        {
            try
            {
                var posts = (await postService.GetAllPostsAsync())
                    .Where(p => p.Status == PostStatus.Published && p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.PublishedDate)
                    .ToList();
                return Results.Ok(posts);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapGet("/posts/search", async (string q, [FromServices] PostService postService) =>
        {
            try
            {
                var posts = (await postService.GetAllPostsAsync())
                    .Where(p => p.Status == PostStatus.Published &&
                        (p.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                         p.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                         p.Body.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(p => p.PublishedDate)
                    .ToList();
                return Results.Ok(posts);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapPost("/posts", [Authorize(Roles = "Author,Admin")] async (BlogPost post, HttpContext context, [FromServices] PostService postService, [FromServices] RssService rssService) =>
        {
            try
            {
                var user = context.Items["User"] as User;
                if (user == null) return Results.Unauthorized();

                if (!SlugService.IsValidSlug(post.Slug))
                    return Results.BadRequest("Slug must be in kebab-case (lowercase, hyphen-separated).");

                var existing = await postService.GetPostBySlugAsync(post.Slug);
                if (existing != null)
                    return Results.Conflict("Slug already exists.");

                post.Author = user.Username;
                post.PublishedDate = DateTime.UtcNow;
                post.ModifiedDate = DateTime.UtcNow;

                await postService.SavePostAsync(post);

                if (post.Status == PostStatus.Published)
                    await rssService.GenerateRssFeedAsync();

                return Results.Created($"/posts/{post.Slug}", post);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapPut("/posts/{slug}", [Authorize(Roles = "Author,Editor,Admin")] async (string slug, BlogPost updatedPost, HttpContext context, [FromServices] PostService postService, [FromServices] RssService rssService) =>
        {
            try
            {
                var user = context.Items["User"] as User;
                if (user == null) return Results.Unauthorized();

                var existing = await postService.GetPostBySlugAsync(slug);
                if (existing == null) return Results.NotFound();

                var isOwner = existing.Author == user.Username;
                var isEditor = user.Roles.Contains(Roles.Editor) || user.Roles.Contains(Roles.Admin);

                if (!isOwner && !isEditor)
                    return Results.Forbid();

                if (updatedPost.Slug != slug)
                {
                    if (!SlugService.IsValidSlug(updatedPost.Slug))
                        return Results.BadRequest("Slug must be in kebab-case (lowercase, hyphen-separated).");

                    var slugExists = await postService.GetPostBySlugAsync(updatedPost.Slug);
                    if (slugExists != null)
                        return Results.Conflict("Slug already exists.");
                }

                updatedPost.Author = existing.Author;
                updatedPost.PublishedDate = existing.PublishedDate;
                updatedPost.ModifiedDate = DateTime.UtcNow;

                await postService.SavePostAsync(updatedPost);

                if (updatedPost.Status == PostStatus.Published)
                {
                    await rssService.GenerateRssFeedAsync();
                }
                return Results.Ok(updatedPost);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });

        app.MapDelete("/posts/{slug}", [Authorize(Roles = "Admin")] async (string slug, [FromServices] PostService postService) =>
        {
            try
            {
                var post = await postService.GetPostBySlugAsync(slug);
                if (post == null) return Results.NotFound();

                await postService.DeletePostAsync(slug);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred: {ex.Message}");
            }
        });
    }
}