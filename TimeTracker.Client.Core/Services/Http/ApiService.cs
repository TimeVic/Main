using System.Net;
using Fluxor;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.Http.Client;
using TimeTracker.Client.Core.Services.Http.Middleware;

namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService: IApiService, IDisposable
    {
        private readonly CustomHttpClient _httpClient;
        private readonly HttpInterceptorService _httpInterceptorService;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            CustomHttpClient httpClient,
            HttpInterceptorService httpInterceptorService,
            ILogger<ApiService> logger
        )
        {
            _httpClient = httpClient;
            _httpInterceptorService = httpInterceptorService;
            _logger = logger;
            _httpInterceptorService.Register();
        }
        
        private async Task<TResponse?> PostAsync<TResponse>(string requestUri, object? data = null)
        {
            return await _httpClient.RequestAsync<TResponse>(requestUri, data, HttpMethod.Post);
        }
        
        private async Task<TResponse?> GetAsync<TResponse>(string requestUri)
        {
            return await _httpClient.RequestAsync<TResponse>(requestUri, null,  HttpMethod.Get);
        }
        
        private async Task<string?> GetAsync(string requestUri, object data)
        {
            return await _httpClient.RequestAsync(requestUri, data, HttpMethod.Get);
        }
        
        private async Task<TResponse?> MultipartFormDataRequestAsync<TResponse>(
            string requestUri,
            Dictionary<string, object>? data = null,
            IBrowserFile? file = null
        )
        {
            return await _httpClient.MultipartFormDataRequestAsync<TResponse>(
                requestUri,
                data: data,
                file: file
            );
        }
        
        public void Dispose()
        {
            _httpInterceptorService.Unregister();
        }
    }
}
