using FileBlogSystem.Features;
using FileBlogSystem.Services;
using FileBlogSystem.Config;
using FileBlogSystem.Endpoints;

var builder = WebApplication.CreateBuilder(args);


builder.ConfigureServices();

var app = builder.Build();


var userService = app.Services.GetRequiredService<UserService>();
var adminUser = userService.GetUser("admin");
var adminSalma = userService.GetUser("salma");

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

if (adminSalma == null)
{
     adminSalma = new User
    {
        Username = "salma",
        Password = UserService.ComputeHash("salma123"),
        Email = "salma@example.com",
        Roles = new List<string> { "Admin" }
    };
    userService.SaveUser(adminSalma);
}

app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("Welcome to FileBlogSystem!"));
app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapMediaEndpoints();

app.Run();