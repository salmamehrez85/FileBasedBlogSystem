using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enable static file serving (for wwwroot)
builder.Services.AddDirectoryBrowser();

// JWT Authentication Configuration (placeholder secret key)
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

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Example route
app.MapGet("/", () => Results.Ok("Welcome to FileBlogSystem!"));

// Placeholder for future endpoints
// app.MapPost("/api/posts", ...);
// app.MapGet("/api/posts", ...);

app.Run();
