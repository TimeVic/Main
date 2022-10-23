using Newtonsoft.Json;
using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Business.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<T?> GetJsonDataAsync<T>(this HttpResponseMessage response)
        {
            var stringData = await response.GetDataAsStringAsync();
            try
            {
                return JsonHelper.DeserializeObject<T>(stringData, DateTimeZoneHandling.Local);
            }
            catch (Exception _)
            {
                return default;
            }
        }
        
        public static async Task<object?> GetJsonDataAsync(this HttpResponseMessage response)
        {
            return await response.GetJsonDataAsync<object>();
        }

        public static async Task<string> GetDataAsStringAsync(this HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
}
