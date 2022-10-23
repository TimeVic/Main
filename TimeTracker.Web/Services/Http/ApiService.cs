using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fluxor;
using Newtonsoft.Json;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http.Dto;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService: IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public ApiService(
            HttpClient httpClient,
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _apiUrl = _configuration.GetValue<string>("ApiUrl");
        }

        public string? GetJwt()
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            return store?.Value.Jwt;
        }
        
        private async Task<string> RequestAsync(string requestUri, string jwtToken, object data, HttpMethod httpMethod)
        {   
            // create request object
            var request = new HttpRequestMessage(httpMethod, $"{_apiUrl}/{requestUri}");
            if (
                httpMethod == HttpMethod.Post
                || httpMethod == HttpMethod.Put
            )
            {
                data ??= new { };
                request.Content = JsonContent.Create(data);    
            }
            // add authorization header
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            // send request
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return responseString;
            }

            BadResponseDto? badResponse = null;
            try
            {
                badResponse = JsonHelper.DeserializeObject<BadResponseDto>(
                    responseString,
                    DateTimeZoneHandling.Local
                );
            }
            finally
            {
                throw new HttpResponseException(
                    response.StatusCode,
                    badResponse?.Message ?? "Server error",
                    badResponse?.Type
                );
            }
        }
        
        private async Task<TResponse?> RequestAsync<TResponse>(string requestUri, string jwtToken, object data, HttpMethod httpMethod)
        {
            var responseString = await RequestAsync(requestUri, jwtToken, data, httpMethod);
            return JsonHelper.DeserializeObject<TResponse>(
                responseString,
                DateTimeZoneHandling.Local
            );
        }

        private async Task<TResponse?> PostAsync<TResponse>(string requestUri, object data, string jwtToken = null)
        {
            return await RequestAsync<TResponse>(requestUri, jwtToken, data, HttpMethod.Post);
        }
        
        private async Task<TResponse?> PostAuthorizedAsync<TResponse>(string requestUri, object data = null)
        {
            return await RequestAsync<TResponse>(
                requestUri, 
                GetJwt(), 
                data, 
                HttpMethod.Post
            );
        }
        
        private async Task PostAuthorizedAsync(string requestUri, object data)
        {
            await RequestAsync(
                requestUri, 
                GetJwt(), 
                data, 
                HttpMethod.Post
            );
        }
        
        private async Task<TResponse> GetAsync<TResponse>(string requestUri)
        {
            return await RequestAsync<TResponse>(requestUri, null, null, HttpMethod.Get);
        }
        
        private async Task<string> GetAsync(string requestUri, object data, string jwtToken = null)
        {
            return await RequestAsync(requestUri, jwtToken, data, HttpMethod.Get);
        }
    }
}
