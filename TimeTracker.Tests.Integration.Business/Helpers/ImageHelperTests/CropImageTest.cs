using SixLabors.ImageSharp;
using TimeTracker.Business.Helpers;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Helpers.ImageHelperTests;

public class CropImageTest: BaseTest
{
    public CropImageTest(): base()
    {
    }

    [Fact]
    public async Task ShouldCropImage()
    {
        var imagePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "stubs",
            "images",
            "image.jpg"
        );
        var actualImage = await ImageHelper.CropImageFromStreamAsync(imagePath, 100, 128);
        Assert.Equal(100, actualImage.Width);
        Assert.Equal(62, actualImage.Height);
        
        actualImage = await ImageHelper.CropImageFromStreamAsync(imagePath, 250, 100);
        Assert.Equal(162, actualImage.Width);
        Assert.Equal(100, actualImage.Height);
    }
}
