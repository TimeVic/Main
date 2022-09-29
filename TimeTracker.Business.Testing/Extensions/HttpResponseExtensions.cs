using AspNetCore.ApiControllers.Abstractions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Business.Testing.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<BadResponseModel> GetJsonErrorAsync(this HttpResponseMessage response)
        {
            return await response.GetJsonDataAsync<BadResponseModel>();
        }
    }
}
