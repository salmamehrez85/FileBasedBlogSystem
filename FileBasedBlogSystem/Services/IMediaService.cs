using Microsoft.AspNetCore.Http;

namespace FileBlogSystem.Services;

public interface IMediaService
{
    Task SaveAndResizeImageAsync(IFormFile file, string targetDirectory);
} 