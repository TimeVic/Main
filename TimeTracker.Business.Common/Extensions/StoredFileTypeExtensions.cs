using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Business.Common.Extensions;

public static class StoredFileTypeExtensions
{
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
                "text/xml",
                "audio/aac",
                "application/msaccess",
                "application/x-7z-compressed",
                "image/jpeg",
                "image/png",
                "image/bmp",
                "image/gif",
                "text/plain",
                "application/xml",
                "text/css",
                "text/csv",
                "application/x-gzip",
                "application/x-gtar",
                "image/pjpeg",
                "application/x-javascript",
                "application/json",
                "video/mpeg",
                "audio/x-mpegurl",
                "audio/m4a",
                "audio/m4b",
                "audio/m4p",
                "audio/x-m4r",
                "video/x-m4v",
                "image/x-macpaint",
                "application/x-msaccess",
                "audio/mid",
                "video/quicktime",
                "video/mp4",
                "application/onenote",
                "image/pict",
                "application/vnd.ms-powerpoint",
                "application/vnd.ms-powerpoint.template.macroEnabled.12",
                "application/vnd.openxmlformats-officedocument.presentationml.template",
                "image/x-portable-pixmap",
                "audio/x-pn-realaudio",
                "image/x-cmu-raster",
                "image/vnd.rn-realflash",
                "image/x-rgb",
                "text/richtext",
                "text/x-ms-rqy",
                "text/scriptlet",
                "text/sgml",
                "text/html",
                "audio/x-smd",
                "audio/basic",
                "application/x-tar",
                "application/x-compressed",
                "image/tiff",
                "application/vnd.ms-visio.viewer",
                "application/vnd.visio",
                "audio/wav",
                "image/vnd.ms-photo",
                "application/x-safari-webarchive",
                "video/x-ms-wm",
                "audio/x-ms-wma",
                "application/x-ms-wmd",
                "text/vnd.wap.wml",
                "video/x-ms-wmp",
                "video/x-ms-wmv",
                "video/x-ms-wmx",
                "image/x-xbitmap",
                "image/x-xpixmap",
                "application/x-compress",
                "application/x-zip-compressed",
                "image/webp",
            };
        }
        return new List<string>();
    }
}
