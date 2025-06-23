namespace FileBlogSystem.Services;

public class MediaService
{
    public async Task SaveAndResizeImageAsync(IFormFile file, string targetDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was uploaded.");

        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
            throw new ArgumentException("File size exceeds the 5MB limit.");


        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedContentTypes.Contains(file.ContentType))
            throw new ArgumentException("Invalid file type. Only JPG, PNG, and GIF are allowed.");

        Directory.CreateDirectory(targetDirectory);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(targetDirectory, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (IsImageFile(file.FileName))
        {
            using (var image = await Image.LoadAsync(filePath))
            {
                var maxWidth = 1200;
                var maxHeight = 800;

                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));

                    await image.SaveAsync(filePath);
                }
            }
        }
    }

    private bool IsImageFile(string fileName)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        return allowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
    }
} 