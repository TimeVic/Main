using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Services;
using HttpClient = System.Net.Http.HttpClient;

namespace TimeTracker.Tests.Integration.Api.Core;

public class BaseTest: IClassFixture<ApiCustomWebApplicationFactory>, IDisposable
{
    protected readonly ApiCustomWebApplicationFactory _factory;
    
    protected readonly IServiceProvider ServiceProvider;
    protected readonly HttpClient HttpClient;
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly IUserSeeder UserSeeder;
    protected readonly IDataFactory<UserEntity> UserFactory;
    protected readonly SmtpClientServiceMock SmtpClientServiceMock;
    private readonly IDbCleanUpService _dbCleanUpService;
    protected readonly FirebaseClientServiceMock FirebaseClientService;
    protected readonly IQueueDao _queueDao;
    private readonly IQueueService _queueService;

    public BaseTest(ApiCustomWebApplicationFactory factory)
    {
        _factory = factory;
        HttpClient = _factory.CreateClient();
        
        DbSessionProvider = _factory.Services.GetRequiredService<IDbSessionProvider>();
        _dbCleanUpService = _factory.Services.GetRequiredService<IDbCleanUpService>();
        UserSeeder = _factory.Services.GetRequiredService<IUserSeeder>();
        UserFactory = _factory.Services.GetRequiredService<IDataFactory<UserEntity>>();
        _queueDao = _factory.Services.GetRequiredService<IQueueDao>();
        _queueService = _factory.Services.GetRequiredService<IQueueService>();
        SmtpClientServiceMock = (_factory.Services.GetRequiredService<ISmtpClientService>() as SmtpClientServiceMock)!;
        FirebaseClientService = (_factory.Services.GetRequiredService<IFirebaseClientService>() as FirebaseClientServiceMock)!;
        ServiceProvider = _factory.Services;

        _dbCleanUpService.CleanUp().Wait();
    }
    
    public void Dispose()
    {
        FlushDbChanges().Wait();
        GC.SuppressFinalize(this);
    }

    protected async Task FlushDbChanges(bool isClearSession = false)
    { 
        await DbSessionProvider.CurrentSession.FlushAsync();
        if (isClearSession)
        {
            DbSessionProvider.CurrentSession.Clear();
        }
    }
    
    protected async Task RefreshEntity(object obj)
    {
        await DbSessionProvider.CurrentSession.RefreshAsync(obj);
    }
    
    protected async Task FlushAndRefreshEntity(object obj, bool isClearSession = false)
    {
        await FlushDbChanges(isClearSession);
        await DbSessionProvider.CurrentSession.RefreshAsync(obj);
    }
    
    protected async Task<int> QueueProcess(QueueChannel channel)
    {
        await FlushDbChanges();
        _queueDao.Flush();
        await _queueDao.UpdateProcessAtForPending();
        return await _queueService.ProcessAsync(channel, isClearSessionForEachIteration: false);
    }
    
    #region Http
    public async Task<HttpResponseMessage> PostRequestAsAnonymousAsync(string url, object data = null)
    {
        await FlushDbChanges();

        var requestData = JsonContent.Create(data ?? new { });
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> PostRequestAsync(string url, string jwtToken,  object data = null)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var requestData = JsonContent.Create(data ?? new {});
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsAnonymousAsync(
        string url,
        Dictionary<string, string>? urlParams = null
    )
    {
        urlParams ??= new Dictionary<string, string>();
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Relative);
        await FlushDbChanges();

        return await HttpClient.GetAsync(uri);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsync(string url, string jwtToken, Dictionary<string, string>? urlParams = null)
    {
        await FlushDbChanges();

        urlParams ??= new Dictionary<string, string>();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "text/json");
        
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Relative);
        return await HttpClient.GetAsync(uri);
    }
    
    public async Task<HttpResponseMessage> PostMultipartFormDataRequestAsync(
        string url,
        string? token = null,
        Dictionary<string, object> data = null,
        IFormFile file = null
    )
    {
        await FlushDbChanges();

        if (!string.IsNullOrEmpty(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);    
        }
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
            var fileStreamContent = new StreamContent(file.OpenReadStream());
            multipartFormContent.Add(fileStreamContent, name: "File", fileName: file.FileName);
        }
        return await HttpClient.PostAsync(url, multipartFormContent);
    }
    #endregion
    
    #region Uploading

    protected IFormFile CreateFormFile(string fileName = "test.pdf", byte[]? fileBytes = null)
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var stream = new MemoryStream();
            
        var stubsPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        stubsPath = Path.GetDirectoryName(stubsPath);
        stubsPath = Path.Combine(stubsPath, "stubs");
        if (fileBytes != null)
        {
            stream.Write(fileBytes);
        }
        else
        {
            var filePath = Path.Combine(stubsPath, fileName);
            if (File.Exists(filePath))
            {
                var stubFileBytes = File.ReadAllBytes(filePath);
                stream.Write(stubFileBytes);
            }
            else
            {
                var content = "Hello World from a Fake File";
                stream.Write(Encoding.UTF8.GetBytes(content));
            }
        }
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "id_from_form", fileName);
    }

    #endregion
}
