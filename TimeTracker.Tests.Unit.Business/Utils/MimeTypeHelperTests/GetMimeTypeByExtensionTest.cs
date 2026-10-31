using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Tests.Unit.Business.Utils.MimeTypeHelperTests;

public class GetMimeTypeByExtensionTest
{
    [Theory]
    [InlineData("svg", "image/svg+xml")]
    [InlineData(".svg", "image/svg+xml")]
    [InlineData("SVG", "image/svg+xml")]
    [InlineData(".SVG", "image/svg+xml")]
    [InlineData("svgz", "image/svg+xml")]
    [InlineData(".svgz", "image/svg+xml")]
    [InlineData("png", "image/png")]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("pdf", "application/pdf")]
    public void ShouldResolveMimeTypeByExtension(string extension, string expectedMimeType)
    {
        var actualMimeType = MimeTypeHelper.GetMimeTypeByExtension(extension);
        Assert.Equal(expectedMimeType, actualMimeType);
    }

    [Theory]
    [InlineData("icon.svg", "image/svg+xml")]
    [InlineData("path/to/diagram.svg", "image/svg+xml")]
    [InlineData("photo.jpg", "image/jpeg")]
    public void ShouldResolveMimeTypeByName(string fileName, string expectedMimeType)
    {
        var actualMimeType = MimeTypeHelper.GetMimeTypeByName(fileName);
        Assert.Equal(expectedMimeType, actualMimeType);
    }
}

