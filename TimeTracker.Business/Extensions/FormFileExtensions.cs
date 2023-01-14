using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions;
using TimeTracker.Business.Common.Exceptions.Api;

namespace TimeTracker.Business.Extensions;

public static class FormFileExtensions
{
    public static string GetExtension(this IFormFile file)
    {
        var fileExt = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(fileExt))
        {
            throw new IncorrectFileException("Invalid file extension");
        }
        var parts = fileExt.Split('.');
        if (parts.Length > 0)
        {
            return parts[1].ToLower();
        }
        return parts[0].ToLower();
    }
}
