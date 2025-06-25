using FileBlogSystem.Features;

namespace FileBlogSystem.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(string username);
    Task SaveUserAsync(User user);
    Task<bool> ValidateUserAsync(string username, string password);
} 