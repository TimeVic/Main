using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Business.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<T?> GetJsonDataAsync<T>(this HttpResponseMessage response)
        {
            var stringData = await response.GetDataAsStringAsync();
            return JsonHelper.DeserializeObject<T>(stringData);
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
