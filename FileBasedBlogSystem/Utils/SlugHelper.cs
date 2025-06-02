using System.Text.RegularExpressions;

namespace FileBlogSystem.Utils;

public static class SlugHelper
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
