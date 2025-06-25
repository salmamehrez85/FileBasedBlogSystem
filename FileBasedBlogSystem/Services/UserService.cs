using FileBlogSystem.Features;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace FileBlogSystem.Services;

public class UserService : IUserService
{
    private readonly string _usersRoot = Path.Combine("Content", "Users");

    public async Task<User?> GetUserAsync(string username)
    {
        var userPath = Path.Combine(_usersRoot, username, "profile.json");
        if (!File.Exists(userPath)) return null;

        var json = await File.ReadAllTextAsync(userPath);
        return JsonSerializer.Deserialize<User>(json);
    }

    public async Task SaveUserAsync(User user)
    {
        var userDir = Path.Combine(_usersRoot, user.Username);
        Directory.CreateDirectory(userDir);

        var userPath = Path.Combine(userDir, "profile.json");
        var json = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(userPath, json);
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var user = await GetUserAsync(username);
        if (user == null) return false;

        var hash = ComputeHash(password);
        return user.Password == hash;
    }

    public static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
