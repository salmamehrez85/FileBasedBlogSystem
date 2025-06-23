using FileBlogSystem.Features;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FileBlogSystem.Services;

public class JwtAuthService
{
    private readonly RequestDelegate _next;
    private readonly UserService _userService;
    private readonly IConfiguration _config;

    public JwtAuthService(RequestDelegate next, UserService userService, IConfiguration config)
    {
        _next = next;
        _userService = userService;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var username = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _userService.GetUserAsync(username);
                if (user != null)
                {
                    context.Items["User"] = user;
                }
            }
        }

        await _next(context);
    }

} 