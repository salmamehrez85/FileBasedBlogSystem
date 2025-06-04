namespace FileBlogSystem.Services;

public class MediaService
{
    public async Task SaveAndResizeImageAsync(IFormFile file, string targetDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was uploaded.");


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