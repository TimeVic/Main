using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;

namespace TimeTracker.Tests.Unit.Business.Extensions.StoredFileTypeExtensionsTests;

public class GetAllowedMimeTypesTest
{
    [Fact]
    public void ShouldIncludeSvgInAllowedAttachmentMimeTypes()
    {
        var allowedMimeTypes = StoredFileType.Attachment.GetAllowedMimeTypes();

        Assert.Contains("image/svg+xml", allowedMimeTypes);
        Assert.Contains("image/png", allowedMimeTypes);
        Assert.Contains("image/jpeg", allowedMimeTypes);
        Assert.Contains("application/pdf", allowedMimeTypes);
    }
}

