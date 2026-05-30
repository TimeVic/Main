using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Client.Core.Converters.Json;
using TimeTracker.Client.Core.Services.DateTimes;
using TimeTracker.Client.Core.Services.Http.Cookies;
using TimeTracker.Client.Core.Services.Http.Dto;

namespace TimeTracker.Client.Core.Services.Http.Client;

public class CustomHttpClient
{
    private readonly string _apiUrl;
    private readonly int _maxFileSizeInMb;
    
    private readonly HttpClient _httpClient;
    private readonly UserDateTimeProviderService _dateTimeProviderService;
    private readonly IAuthCookieConfigurator _authCookieConfigurator;
    private readonly ILogger<CustomHttpClient> _logger;

    public CustomHttpClient(
        HttpClient httpClient,
        IConfiguration configuration,
        UserDateTimeProviderService dateTimeProviderService,
        IAuthCookieConfigurator authCookieConfigurator,
        ILogger<CustomHttpClient> logger
    )
    {
        _httpClient = httpClient;
        _dateTimeProviderService = dateTimeProviderService;
        _authCookieConfigurator = authCookieConfigurator;
        _logger = logger;

        _apiUrl = configuration.GetValue<string>("ApiUrl")!;
        _maxFileSizeInMb = configuration.GetValue<int>("MaxFileSize");
    }
    
    public async Task<TResponse?> RequestAsync<TResponse>(string requestUri, object data, HttpMethod httpMethod)
    {
        var responseString = await RequestAsync(requestUri, data, httpMethod);
        return Deserialize<TResponse>(responseString);
    }
    
    public async Task<string> RequestAsync(string requestUri, object data, HttpMethod httpMethod)
    {   
        // create request object
        var request = new HttpRequestMessage(httpMethod, $"{_apiUrl}/{requestUri}");
        await _authCookieConfigurator.ConfigureRequestAsync(request);
        if (
            httpMethod == HttpMethod.Post
            || httpMethod == HttpMethod.Put
        )
        {
            data ??= new { };
            request.Content = new StringContent(
                JsonHelper.SerializeToString(data, DateTimeZoneHandling.Utc), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );
        }

        // send request
        var response = await _httpClient.SendAsync(request);
        await _authCookieConfigurator.ProcessResponseAsync(response);
        return await HandleHttpResponse(response);
    }
    
    public async Task<TResponse?> MultipartFormDataRequestAsync<TResponse>(
        string requestUri,
        Dictionary<string, object>? data = null,
        IBrowserFile? file = null
    )
    {   
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/{requestUri}");
        await _authCookieConfigurator.ConfigureRequestAsync(request);
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
        
        // send request
        var response = await _httpClient.SendAsync(request);
        await _authCookieConfigurator.ProcessResponseAsync(response);
        var responseString = await HandleHttpResponse(response);
        return Deserialize<TResponse>(responseString);
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
            badResponse = Deserialize<BadResponseDto>(responseString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        finally
        {
            throw new HttpResponseException(
                response.StatusCode,
                badResponse?.Message ?? "Server error",
                badResponse?.Type ?? "HttpResponseException"
            );
        }
    }

    private T? Deserialize<T>(string responseString)
    {
        try
        {
            return JsonHelper.DeserializeObject<T>(
                responseString,
                DateTimeZoneHandling.Local,
                converters: [
                    new UserTimeZoneDateConverter(_dateTimeProviderService.GetTimeZone())
                ],
                contractResolver: new UserTimeZoneDateConverter.ContractResolver()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return default;
    }
}
