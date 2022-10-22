using System.Text;
using Newtonsoft.Json;

namespace TimeTracker.Business.Common.Helpers;

public static class JsonHelper
{
    public static string SerializeToString(object data, DateTimeZoneHandling? dateTimeZoneHandling = null)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
        {
            DateTimeZoneHandling = dateTimeZoneHandling ?? DateTimeZoneHandling.Utc
        });
    }
    
    public static byte[] SerializeToBytes(object data)
    {
        var jsonString = SerializeToString(data);
        if (!string.IsNullOrEmpty(jsonString))
        {
            return Encoding.UTF8.GetBytes(jsonString);
        }

        return null;
    }
    
    public static T? DeserializeObject<T>(string value, DateTimeZoneHandling? dateTimeZoneHandling = null)
    {
        return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings()
        {
            DateTimeZoneHandling = dateTimeZoneHandling ?? DateTimeZoneHandling.Utc
        });
    }
}
