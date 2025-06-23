using FileBlogSystem.Features;
using System.Text.Json;

namespace FileBlogSystem.Services;

public class ConfigService
{
    private readonly string _siteConfigPath = Path.Combine("Config", "site.json");
    private readonly string _routesConfigPath = Path.Combine("Config", "routes.json");

    public async Task<SiteConfig> GetSiteConfigAsync()
    {
        if (!File.Exists(_siteConfigPath))
            return new SiteConfig();

        var json = await File.ReadAllTextAsync(_siteConfigPath);
        return JsonSerializer.Deserialize<SiteConfig>(json) ?? new SiteConfig();
    }

    public async Task<Dictionary<string, string>> GetCustomRoutesAsync()
    {
        if (!File.Exists(_routesConfigPath))
            return new Dictionary<string, string>();

        var json = await File.ReadAllTextAsync(_routesConfigPath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
    }

   
    public Dictionary<string, string> GetCustomRoutes()
    {
        if (!File.Exists(_routesConfigPath))
            return new Dictionary<string, string>();

        var json = File.ReadAllText(_routesConfigPath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
    }
}
