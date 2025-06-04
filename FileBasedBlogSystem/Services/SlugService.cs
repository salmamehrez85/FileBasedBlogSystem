using System.Text.RegularExpressions;

namespace FileBlogSystem.Services;

public static class SlugService
{
    public static bool IsValidSlug(string slug)
    {
        return Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$");
    }

    public static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
} 