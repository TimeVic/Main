using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace TimeTracker.Business.Helpers;

public static class ImageHelper
{
    public static async Task<Image> ResizeImageFromStreamAsync(string path, int width, int height)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return await ResizeImageFromStreamAsync(fileStream, width, height);
    }
    
    public static Task<Image> ResizeImageFromStreamAsync(Stream imageStream, int width, int height)
    {
        return ResizeImageFromStreamAsync(imageStream, width, height, ResizeMode.Max);
    }
    
    public static async Task<Image> ResizeImageFromStreamAsync(
        Stream imageStream,
        int width,
        int height,
        ResizeMode resizeMode,
        bool isGrayscale = true
    )
    {
        var image = await Image.LoadAsync(imageStream);
        image.Mutate(x =>
        {
            x.AutoOrient()
                .Resize(new ResizeOptions
                {
                    Mode = resizeMode,
                    Position = AnchorPositionMode.Center,
                    Size = new Size(width, height)
                });

            if (isGrayscale)
            {
                x.Grayscale();
            }
        });
        return image;
    }
    
    public static async Task<bool> IsImage(byte[] imageBytes)
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);
            await Image.LoadAsync(stream);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}
