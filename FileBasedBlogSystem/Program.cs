using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileBlogSystem.Utils;
using FileBlogSystem.Models;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDirectoryBrowser();


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



app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();


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



app.Run();
