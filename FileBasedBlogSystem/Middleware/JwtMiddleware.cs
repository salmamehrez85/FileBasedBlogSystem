using FileBlogSystem.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FileBlogSystem.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var username = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = userService.GetUser(username);
                if (user != null)
                {
                    context.Items["User"] = user;
                }
            }
        }

        await _next(context);
    }
}
