using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Business.Extensions;

public static class StoredFileTypeExtensions
{
    public static string GetSubDirectory(this StoredFileType type)
    {
        if (type == StoredFileType.Image)
        {
            return "image";
        }
        else if (type == StoredFileType.Attachment)
        {
            return "attachment";
        }
        else if (type == StoredFileType.Avatar)
        {
            return "avatar";
        }
        return "";
    }
    
    public static string GetFilePath(this StoredFileType type, string fileExtension)
    {
        return $"{type.GetSubDirectory()}/{DateTime.Now.Year}/{DateTime.Now.Month}/{Guid.NewGuid()}.{fileExtension}";
    }
}
