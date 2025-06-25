using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileBlogSystem.Services;

namespace FileBlogSystem.Config;

public static class ServiceConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPostService, PostService>();
        builder.Services.AddSingleton<IRssService, RssService>();
        builder.Services.AddSingleton<IConfigService, ConfigService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IMediaService, MediaService>();


        // Configure JWT Authentication
        var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "a1b2c3d4e5f67890123456789abcdef0fedcba9876543210abcdef1234567890";
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyIssuer";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MyAudience";
        var jwtExpireMinutes = double.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var exp) ? exp : 60;

        builder.Services.AddSingleton<JwtTokenService>(sp =>
            new JwtTokenService(jwtSecretKey, jwtIssuer, jwtAudience, jwtExpireMinutes)
        );

        // Configure Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
            };
        });

        // Configure Authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("AuthorOrEditor", policy => policy.RequireRole("Author", "Editor"));
        });
    }
} 