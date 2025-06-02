using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileBlogSystem.Utils;
using FileBlogSystem.Models;
using FileBlogSystem.Middleware;

var builder = WebApplication.CreateBuilder(args);




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

app.MapGet("/api/admin", () => Results.Ok("Admin area"))
   .RequireAuthorization("AdminOnly");

app.MapGet("/api/editor", () => Results.Ok("Author or Editor area"))
   .RequireAuthorization("AuthorOrEditor");

app.Run();
