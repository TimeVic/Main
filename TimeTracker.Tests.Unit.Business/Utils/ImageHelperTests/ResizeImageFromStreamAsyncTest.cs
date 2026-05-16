using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Utils.ImageHelperTests;

public class ResizeImageFromStreamAsyncTest
{
    [Fact]
    public async Task ShouldCropImageToExactSize()
    {
        using var stream = await CreateImageStreamAsync(400, 200);
        
        using var actualImage = await ImageHelper.ResizeImageFromStreamAsync(
            stream,
            128,
            128,
            ResizeMode.Crop,
            isGrayscale: false
        );
        
        Assert.Equal(128, actualImage.Width);
        Assert.Equal(128, actualImage.Height);
    }
    
    [Fact]
    public async Task ShouldResizeImageWithinMaxSizeByDefault()
    {
        using var stream = await CreateImageStreamAsync(400, 200);
        
        using var actualImage = await ImageHelper.ResizeImageFromStreamAsync(stream, 128, 128);
        
        Assert.Equal(128, actualImage.Width);
        Assert.Equal(64, actualImage.Height);
    }

    private static async Task<MemoryStream> CreateImageStreamAsync(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (var x = 0; x < pixelRow.Length; x++)
                {
                    pixelRow[x] = new Rgba32(120, 80, 40);
                }
            }
        });

        var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        return stream;
    }
}
