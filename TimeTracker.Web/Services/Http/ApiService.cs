using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fluxor;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http.Dto;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        
        private readonly string _apiUrl;
        private readonly int _maxFileSizeInMb;

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
            _maxFileSizeInMb = _configuration.GetValue<int>("MaxFileSize");
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
            return await HandleHttpResponse(response);
        }
        
        private async Task<TResponse?> MultipartFormDataRequestAsync<TResponse>(
            string requestUri,
            Dictionary<string, object> data = null,
            IBrowserFile file = null
        )
        {   
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/{requestUri}");
            using var multipartFormContent = new MultipartFormDataContent();
            if (data != null)
            {
                foreach (var dataKeyPair in data)
                {
                    multipartFormContent.Add(new StringContent($"{dataKeyPair.Value}"), name: dataKeyPair.Key);       
                }
            }
            if (file != null)
            {
                var maxSize = _maxFileSizeInMb * 1024 * 1024;
                if (file.Size > maxSize)
                {
                    throw new Exception($"The file size cannot be larger than {_maxFileSizeInMb} Mb");
                }

                var fileStreamContent = new StreamContent(file.OpenReadStream(maxSize));
                multipartFormContent.Add(fileStreamContent, name: "File", fileName: file.Name);
            }
            request.Content = multipartFormContent;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetJwt());

            // send request
            var response = await _httpClient.SendAsync(request);
            var responseString = await HandleHttpResponse(response);
            return JsonHelper.DeserializeObject<TResponse>(
                responseString,
                DateTimeZoneHandling.Local
            );
        }

        private async Task<string> HandleHttpResponse(HttpResponseMessage response)
        {
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
            var response = JsonHelper.DeserializeObject<TResponse>(
                responseString,
                DateTimeZoneHandling.Local
            );
            return response;
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
