using FileBlogSystem.Features;
using FileBlogSystem.Services;
using FileBlogSystem.Config;
using FileBlogSystem.Endpoints;

var builder = WebApplication.CreateBuilder(args);


builder.ConfigureServices();

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


app.Use(async (context, next) =>
{
    var jwtAuthService = new JwtAuthService(next, userService, builder.Configuration);
    await jwtAuthService.InvokeAsync(context);
});


app.MapGet("/", () => Results.Ok("Welcome to FileBlogSystem!"));
app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapMediaEndpoints();

app.Run();