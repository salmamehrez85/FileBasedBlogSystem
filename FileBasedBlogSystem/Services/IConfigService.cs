using FileBlogSystem.Features;

namespace FileBlogSystem.Services;

public interface IConfigService
{
    Task<SiteConfig> GetSiteConfigAsync();
    Task<Dictionary<string, string>> GetCustomRoutesAsync();
    Dictionary<string, string> GetCustomRoutes();
} 