using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileBlogSystem.Utils;
using FileBlogSystem.Models;
using FileBlogSystem.Middleware;
using FileBlogSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FileBlogSystem.Services.PostService>();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<RssService>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<FileBlogSystem.Services.UserService>();

var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "a1b2c3d4e5f67890123456789abcdef0fedcba9876543210abcdef1234567890";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuthorOrEditor", policy => policy.RequireRole("Author", "Editor"));
});

var app = builder.Build();

var userService = app.Services.GetRequiredService<UserService>();
var adminUser = userService.GetUser("admin");
if (adminUser == null)
{
    adminUser = new User
    {
        Username = "admin",
        Password = UserService.ComputeHash("admin123"),
        Email = "admin@example.com",
        Roles = new List<string> { "Admin" }
    };
    userService.SaveUser(adminUser);
}

app.UseRouting(); 
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

app.MapGet("/", () => Results.Ok("Welcome to FileBlogSystem!"));

app.MapGet("/api/secure-data", () =>
{
    return Results.Ok("This is protected data.");
}).RequireAuthorization();

app.MapPost("/login", (User loginUser, IConfiguration config) =>
{
    if (loginUser.Username == "admin" && loginUser.Password == "admin123")
    {
        var roles = new List<string> { "Admin" };
        var token = JwtTokenHelper.GenerateToken(loginUser.Username, roles, config);
        return Results.Ok(new { token });
    }

    return Results.Unauthorized();
});

app.MapPost("/posts/{slug}/upload", async (
    HttpContext context,
    string slug,
    IFormFile file,
    PostService postService,
    ImageService imageService) =>
{
    var user = context.Items["User"] as User;
    if (user == null) return Results.Unauthorized();

    var post = postService.GetPostBySlug(slug);
    if (post == null) return Results.NotFound("Post not found.");

    if (file == null || file.Length == 0)
        return Results.BadRequest("No file was uploaded.");

    var postDir = Path.Combine("Content", "Posts", $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}", "assets");
    Directory.CreateDirectory(postDir);
    await imageService.SaveAndResizeImageAsync(file, postDir);

    return Results.Ok("Image uploaded and resized.");
}).RequireAuthorization().DisableAntiforgery();

app.MapGet("/admin", () => Results.Ok("Admin area"))
   .RequireAuthorization("AdminOnly");

app.MapGet("/editor", () => Results.Ok("Author or Editor area"))
   .RequireAuthorization("AuthorOrEditor");

app.MapGet("/posts", (int? page, int? pageSize, [FromServices]PostService postService) =>
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



app.MapGet("/posts/{slug}", (string slug, [FromServices] PostService postService, MarkdownService markdownService) =>
{
  
    var post = postService.GetPostBySlug(slug);
    if (post == null) return Results.NotFound();

    var html = markdownService.ConvertToHtml(post.Body);
    return Results.Ok(new { html });

});



app.MapGet("/posts/category/{category}", (string category, [FromServices]PostService postService) =>
{
    var posts = postService.GetAllPosts()
        .Where(p => p.Status == "published" && p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
        .OrderByDescending(p => p.PublishedDate);

    return Results.Ok(posts);
});

app.MapGet("/posts/tag/{tag}", (string tag, [FromServices]PostService postService) =>
{
    var posts = postService.GetAllPosts()
        .Where(p => p.Status == "published" && p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .OrderByDescending(p => p.PublishedDate);

    return Results.Ok(posts);
});

app.MapGet("/posts/search", (string q, [FromServices]PostService postService) =>
{
    var posts = postService.GetAllPosts()
        .Where(p => p.Status == "published" &&
            (p.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             p.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             p.Body.Contains(q, StringComparison.OrdinalIgnoreCase)))
        .OrderByDescending(p => p.PublishedDate);

    return Results.Ok(posts);
});

app.MapPost("/posts", [Authorize(Roles = "Author,Admin")] (BlogPost post, HttpContext context, [FromServices]PostService postService, [FromServices]RssService rssService) =>
{
    var user = context.Items["User"] as User;
    if (user == null) return Results.Unauthorized();


    if (!SlugHelper.IsValidSlug(post.Slug))
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

app.MapPut("/posts/{slug}", [Authorize(Roles = "Author,Editor,Admin")] (string slug, BlogPost updatedPost, HttpContext context, [FromServices]PostService postService, [FromServices]RssService rssService) =>
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
        if (!SlugHelper.IsValidSlug(updatedPost.Slug))
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

    var dir = Path.Combine("Content", "Posts", $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}");
    if (Directory.Exists(dir))
        Directory.Delete(dir, true);

    return Results.Ok($"Deleted post: {slug}");
});

app.MapFallback( (HttpContext context, PostService postService, ConfigService configService, MarkdownService markdownService) =>
{
    var path = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();
    if (string.IsNullOrEmpty(path)) return Results.NotFound();

    var routes = configService.GetCustomRoutes();

    if (routes.TryGetValue(path, out var slug))
    {
        var post = postService.GetPostBySlug(slug);
        if (post == null || post.Status != "published")
            return Results.NotFound();

        var html = markdownService.ConvertToHtml(post.Body);
        return Results.Content(html, "text/html");
    }

    return Results.NotFound();
});


app.Run();