using Microsoft.AspNetCore.Mvc;
using FileBlogSystem.Services;
using FileBlogSystem.Features;

namespace FileBlogSystem.Endpoints;

public static class AuthEndpoints
{
   public static void MapAuthEndpoints(this WebApplication app)
{
    app.MapPost("/login", async (User loginUser, [FromServices] JwtTokenService jwtService, [FromServices] IUserService userService) =>
    {
        try
        {
            if (await userService.ValidateUserAsync(loginUser.Username, loginUser.Password))
            {
                var user = await userService.GetUserAsync(loginUser.Username);
                var token = jwtService.GenerateToken(loginUser.Username, user!.Roles.Select(r => r.ToString()).ToList());
                return Results.Ok(new { token });
            }
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    });

    app.MapPost("/register", async (User newUser, [FromServices] IUserService userService) =>
    {
        try
        {
            var existingUser = await userService.GetUserAsync(newUser.Username);
            if (existingUser != null)
            {
                return Results.Conflict("Username already exists.");
            }

            // Hash the password before saving
            newUser.Password = UserService.ComputeHash(newUser.Password);

            // Assign Author role if not provided
            if (newUser.Roles == null || newUser.Roles.Count == 0)
            {
                newUser.Roles = new List<Roles> { Roles.Author };
            }

            await userService.SaveUserAsync(newUser);
            return Results.Created($"/users/{newUser.Username}", newUser);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}");
        }
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