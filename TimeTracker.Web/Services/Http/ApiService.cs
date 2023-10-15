using System.Net;
using Fluxor;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Services.Http.Client;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {   
        private readonly CustomHttpClient _httpClient;
        private readonly IAuthorizationService _authorizationService;

        public ApiService(
            CustomHttpClient httpClient,
            IAuthorizationService authorizationService
        )
        {
            _httpClient = httpClient;
            _authorizationService = authorizationService;
        }

        public string? GetJwt()
        {
            return _authorizationService.GetJwt();
        }
        
        public string? GetAccessToken()
        {
            return _authorizationService.GetAccessToken();
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
                return await _httpClient.RequestAsync<TResponse>(
                    requestUri, 
                    GetJwt(), 
                    data, 
                    HttpMethod.Post
                );
            }
            catch (HttpResponseException responseException)
            {
                if (responseException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        var refreshResponse = await RefreshTokenAsync();
                        if (refreshResponse == null)
                        {
                            // Unauthorized flow
                        }
                        _authorizationService.SetJwt(refreshResponse.JwtToken);
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
