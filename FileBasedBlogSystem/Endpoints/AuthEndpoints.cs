using Microsoft.AspNetCore.Mvc;
using FileBlogSystem.Services;
using FileBlogSystem.Features;

namespace FileBlogSystem.Endpoints;

public static class AuthEndpoints
{
   public static void MapAuthEndpoints(this WebApplication app)
{
    app.MapPost("/login", (User loginUser, [FromServices] JwtTokenService jwtService, [FromServices] UserService userService) =>
    {

        if (userService.ValidateUser(loginUser.Username, loginUser.Password))
        {
            var user = userService.GetUser(loginUser.Username);
            var token = jwtService.GenerateToken(loginUser.Username, user!.Roles);
            return Results.Ok(new { token });
        }
        return Results.Unauthorized();
    });

    app.MapGet("/secure-data", () =>
    {
        return Results.Ok("This is protected data.");
    }).RequireAuthorization();

    app.MapGet("/admin", () => Results.Ok("Admin area"))
       .RequireAuthorization("AdminOnly");

    app.MapGet("/editor", () => Results.Ok("Author or Editor area"))
       .RequireAuthorization("AuthorOrEditor");
}
}