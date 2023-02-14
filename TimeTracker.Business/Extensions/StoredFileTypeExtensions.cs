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
    
    public static ICollection<string> GetAllowedMimeTypes(this StoredFileType type)
    {
        if (type == StoredFileType.Image || type == StoredFileType.Avatar)
        {
            return new List<string>()
            {
                "image/jpeg",
                "image/png",
                "image/bmp",
                "image/gif",
            };
        }
        else if (type == StoredFileType.Attachment)
        {
            return new List<string>()
            {
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-excel",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/pdf",
                "image/jpeg",
                "image/png",
                "image/bmp",
                "image/gif",
            };
        }
        return new List<string>();
    }
    
    public static string GetFilePath(this StoredFileType type, string fileExtension)
    {
        return $"{type.GetSubDirectory()}/{DateTime.Now.Year}/{DateTime.Now.Month}/{Guid.NewGuid()}.{fileExtension}";
    }
}
