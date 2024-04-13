namespace TimeTracker.Console.FileStorage.Util.Services.IO;

public interface ILocalFileSearchService
{
    IEnumerable<string> FindEnumerator(string path, bool isRecursive, string searchMask);
}
