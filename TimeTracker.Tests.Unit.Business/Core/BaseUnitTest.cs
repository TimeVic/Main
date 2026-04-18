using System.Text;

namespace TimeTracker.Tests.Unit.Business.Core;

public class BaseUnitTest
{
    public async Task<string> GetStubString(string name, string? subDir = null)
    {
        var stubsPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        stubsPath = Path.GetDirectoryName(stubsPath);
        stubsPath = Path.Combine(stubsPath!, "stubs", subDir!);
        var filePath = Path.Combine(stubsPath, name);
        var stubFileBytes = await File.ReadAllBytesAsync(filePath);
        return Encoding.UTF8.GetString(stubFileBytes);
    }
}
