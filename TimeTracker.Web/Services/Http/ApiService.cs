using System.Net;
using Fluxor;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http.Client;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {   
        private readonly CustomHttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDispatcher _dispatcher;

        public ApiService(
            CustomHttpClient httpClient,
            IServiceProvider serviceProvider,
            IDispatcher dispatcher
        )
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
            _dispatcher = dispatcher;
        }

        public string? GetJwt()
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            return store?.Value.JwtToken?.Trim();
        }
        
        public string? GetAccessToken()
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            return store?.Value.AccessToken?.Trim();
        }
        
        private async Task<TResponse?> PostAsync<TResponse>(string requestUri, object data, string? jwtToken = null)
        {
            return await _httpClient.RequestAsync<TResponse>(requestUri, jwtToken, data, HttpMethod.Post);
        }
        
        private async Task<TResponse> GetAsync<TResponse>(string requestUri)
        {
            return await _httpClient.RequestAsync<TResponse>(requestUri, null, null, HttpMethod.Get);
        }
        
        private async Task<string> GetAsync(string requestUri, object data, string jwtToken = null)
        {
            return await _httpClient.RequestAsync(requestUri, jwtToken, data, HttpMethod.Get);
        }
        
        #region Authorized
        
        private async Task<TResponse?> PostAuthorizedAsync<TResponse>(string requestUri, object data = null)
        {
            try
            {
                Debug.Log("11111");
                return await _httpClient.RequestAsync<TResponse>(
                    requestUri, 
                    GetJwt(), 
                    data, 
                    HttpMethod.Post
                );
            }
            catch (HttpResponseException responseException)
            {
                Debug.Log("2222");
                if (responseException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        Debug.Log("333333");
                        var refreshResponse = await RefreshTokenAsync();
                        Debug.Log("44444", refreshResponse);
                        if (refreshResponse == null)
                        {
                            // Unauthorized flow
                        }
                        _dispatcher.Dispatch(new SetJwtAction(refreshResponse.JwtToken));
                        _dispatcher.Dispatch(new PersistDataAction());
                    }
                    catch (HttpResponseException responseInnerException)
                    {
                        if (responseInnerException.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // Unauthorized flow
                        }
                    }
                }
            }
            return default;
        }
        
        private async Task PostAuthorizedAsync(string requestUri, object data)
        {
            await _httpClient.RequestAsync(
                requestUri, 
                GetJwt(), 
                data, 
                HttpMethod.Post
            );
        }
        
        private async Task<TResponse?> MultipartFormDataAuthorizedRequestAsync<TResponse>(
            string requestUri,
            Dictionary<string, object>? data = null,
            IBrowserFile? file = null
        )
        {
            return await _httpClient.MultipartFormDataRequestAsync<TResponse>(
                requestUri,
                GetJwt(),
                data: data,
                file: file
            );
        }
        
        #endregion
    }
}
