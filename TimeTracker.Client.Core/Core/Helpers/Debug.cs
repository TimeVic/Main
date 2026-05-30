using Newtonsoft.Json;
using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Client.Core.Core.Helpers;

public static class Debug
{
    public static void Log(params object?[] vals)
    {
        var values = string.Empty;
        foreach (var val in vals)
        {
            values += JsonHelper.SerializeToString(val ?? string.Empty) + " ";    
        }
        Console.WriteLine(values);
    }
}
