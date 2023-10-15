using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Web.Services.Http.Dto;

namespace TimeTracker.Web.Services.Http.Client;

public class CustomHttpClient
{
    private readonly string _apiUrl;
    private readonly int _maxFileSizeInMb;
    
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CustomHttpClient(
        HttpClient httpClient,
        IConfiguration configuration
    )
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        _apiUrl = _configuration.GetValue<string>("ApiUrl");
        _maxFileSizeInMb = _configuration.GetValue<int>("MaxFileSize");
    }
    
    public async Task<TResponse?> RequestAsync<TResponse>(string requestUri, string? jwtToken, object data, HttpMethod httpMethod)
    {
        var responseString = await RequestAsync(requestUri, jwtToken, data, httpMethod);
        var response = JsonHelper.DeserializeObject<TResponse>(
            responseString,
            DateTimeZoneHandling.Local
        );
        return response;
    }
    
    public async Task<string> RequestAsync(string requestUri, string? jwtToken, object data, HttpMethod httpMethod)
    {   
        // create request object
        var request = new HttpRequestMessage(httpMethod, $"{_apiUrl}/{requestUri}");
        if (
            httpMethod == HttpMethod.Post
            || httpMethod == HttpMethod.Put
        )
        {
            data ??= new { };
            request.Content = new StringContent(
                JsonHelper.SerializeToString(data, DateTimeZoneHandling.RoundtripKind), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );
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
    
    public async Task<TResponse?> MultipartFormDataRequestAsync<TResponse>(
        string requestUri,
        string? jwtToken = null,
        Dictionary<string, object>? data = null,
        IBrowserFile? file = null
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
        if (jwtToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);    
        }

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
}
