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



var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "a1b2c3d4e5f67890123456789abcdef0fedcba9876543210abcdef1234567890";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);


builder.Services.AddSingleton<FileBlogSystem.Services.UserService>();

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

app.MapGet("/admin", () => Results.Ok("Admin area"))
   .RequireAuthorization("AdminOnly");

app.MapGet("/editor", () => Results.Ok("Author or Editor area"))
   .RequireAuthorization("AuthorOrEditor");



app.MapGet("/posts", ([FromServices]PostService postService) =>
{
    var posts = postService.GetAllPosts()
        .Where(p => p.Status == "published");
    return Results.Ok(posts);
});


app.MapGet("/posts/{slug}", (string slug, [FromServices]PostService postService) =>
{
    var post = postService.GetPostBySlug(slug);
    return post is not null ? Results.Ok(post) : Results.NotFound();
});


app.MapPost("/posts", [Authorize(Roles = "Author,Admin")] (BlogPost post, HttpContext context, [FromServices]PostService postService) =>
{
    var user = context.Items["User"] as User;
    if (user == null) return Results.Unauthorized();

    post.Author = user.Username;
    post.PublishedDate = DateTime.UtcNow;
    post.ModifiedDate = DateTime.UtcNow;

    postService.SavePost(post);
    return Results.Created($"/api/posts/{post.Slug}", post);
});


app.MapPut("/posts/{slug}", [Authorize(Roles = "Author,Editor,Admin")] (string slug, BlogPost updatedPost, HttpContext context, [FromServices]PostService postService) =>
{
    var user = context.Items["User"] as User;
    if (user == null) return Results.Unauthorized();

    var existing = postService.GetPostBySlug(slug);
    if (existing == null) return Results.NotFound();

    var isOwner = existing.Author == user.Username;
    var isEditor = user.Roles.Contains("Editor") || user.Roles.Contains("Admin");

    if (!isOwner && !isEditor)
        return Results.Forbid();

    updatedPost.Author = existing.Author;
    updatedPost.PublishedDate = existing.PublishedDate;
    updatedPost.ModifiedDate = DateTime.UtcNow;

    postService.SavePost(updatedPost);
    return Results.Ok(updatedPost);
});


app.MapDelete("/posts/{slug}", [Authorize(Roles = "Admin")] (string slug, [FromServices]PostService postService) =>
{
    var post = postService.GetPostBySlug(slug);
    if (post == null) return Results.NotFound();

    var dir = Path.Combine("Content", "Posts", $"{post.PublishedDate:yyyy-MM-dd}-{post.Slug}");
    if (Directory.Exists(dir))
        Directory.Delete(dir, true);

    return Results.Ok($"Deleted post: {slug}");
});






app.Run();
