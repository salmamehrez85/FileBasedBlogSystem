using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FileBlogSystem.Services;

public class ImageService
{
    public async Task SaveAndResizeImageAsync(IFormFile file, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        var ext = Path.GetExtension(file.FileName);
        var originalPath = Path.Combine(targetDir, $"{fileName}{ext}");


        using (var stream = new FileStream(originalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }


        using (var image = await Image.LoadAsync(originalPath))
        {

            var thumbPath = Path.Combine(targetDir, $"{fileName}_thumb{ext}");
            image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(150, 0),
                Mode = ResizeMode.Max
            })).Save(thumbPath);


            var mediumPath = Path.Combine(targetDir, $"{fileName}_medium{ext}");
            image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(600, 0),
                Mode = ResizeMode.Max
            })).Save(mediumPath);
        }
    }
}
