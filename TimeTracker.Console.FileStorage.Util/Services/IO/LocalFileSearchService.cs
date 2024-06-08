using TimeTracker.Business.Extensions;

namespace TimeTracker.Console.FileStorage.Util.Services.IO;

public class LocalFileSearchService: ILocalFileSearchService
{
    public IEnumerable<string> FindEnumerator(string path, bool isRecursive, string searchMask)
    {
        var dirSeparator = Path.DirectorySeparatorChar;
        return Directory.EnumerateFiles(
            $"{AppContext.BaseDirectory}{dirSeparator}{path.RemoveLeadingPathSeparator()}", 
            searchMask, 
            isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );
    }
}
