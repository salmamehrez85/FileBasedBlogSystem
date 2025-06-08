using FileBlogSystem.Features;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;


namespace FileBlogSystem.Services;

public class UserService
{
    private readonly string _usersRoot = Path.Combine("Content", "Users");

    public User? GetUser(string username)
    {
        var userPath = Path.Combine(_usersRoot, username, "profile.json");
        if (!File.Exists(userPath)) return null;

        var json = File.ReadAllText(userPath);
        return JsonSerializer.Deserialize<User>(json);
    }

    public void SaveUser(User user)
    {
        var userDir = Path.Combine(_usersRoot, user.Username);
        Directory.CreateDirectory(userDir);

        var userPath = Path.Combine(userDir, "profile.json");
        File.WriteAllText(userPath, JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true }));
    }


    public bool ValidateUser(string username, string password)
    {
        var user = GetUser(username);
        if (user == null) return false;

        var hash = ComputeHash(password);
         Console.WriteLine($"Computed: {hash}");
        Console.WriteLine($"Stored:   {user.Password}");
        return user.Password == hash;
    }

    public static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

}
