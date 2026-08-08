using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Logging.Client.GrayLog;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Services;
using HttpClient = System.Net.Http.HttpClient;

namespace TimeTracker.Tests.Integration.Api.Core;

public class BaseTest: IClassFixture<ApiCustomWebApplicationFactory>, IDisposable
{
    protected readonly ApiCustomWebApplicationFactory _factory;
    protected IServiceScope ServiceScope;
    
    protected readonly IServiceProvider ServiceProvider;
    protected readonly HttpClient HttpClient;
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly IUserSeeder UserSeeder;
    protected readonly IDataFactory<UserEntity> UserFactory;
    protected readonly GraylogClientMock GraylogClient;
    private readonly IDbCleanUpService _dbCleanUpService;
    protected readonly FirebaseClientServiceMock FirebaseClientService;
    protected readonly IQueueDao _queueDao;
    private readonly IQueueService _queueService;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IUserDao _userDao;

    public BaseTest(ApiCustomWebApplicationFactory factory)
    {
        _factory = factory;
        HttpClient = _factory.CreateClient();
        
        ServiceScope = _factory.Services.CreateScope();
        ServiceProvider = ServiceScope.ServiceProvider;
        
        DbSessionProvider = ServiceProvider.GetRequiredService<IDbSessionProvider>();
        _dbCleanUpService = ServiceProvider.GetRequiredService<IDbCleanUpService>();
        UserSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        UserFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _jwtAuthService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        GraylogClient = (ServiceProvider.GetRequiredService<IGraylogClient>() as GraylogClientMock)!;
        FirebaseClientService = (ServiceProvider.GetRequiredService<IFirebaseClientService>() as FirebaseClientServiceMock)!;

        _dbCleanUpService.CleanUp().Wait();
        GraylogClient.Clear();
    }

    public void Dispose()
    {
        ServiceScope.Dispose();
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
        await _queueDao.Flush();
        await _queueDao.UpdateProcessAtForPending();
        return await _queueService.ProcessAsync(channel, isClearSessionForEachIteration: false);
    }
    
    #region WebSocket
    
    protected HubConnection CreateWebSocketConnection(string url, string? jwtToken = null)
    {
        FlushDbChanges().Wait();
        return new HubConnectionBuilder()
            .WithUrl(
                $"http://localhost/websocket/{url}",
                options =>
                {
                    options.CloseTimeout = TimeSpan.FromMinutes(1);
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    if (!string.IsNullOrEmpty(jwtToken))
                        options.Headers.Add("Authorization", $"Bearer {jwtToken}");
                }
            )
            .Build();
    }
    
    #endregion
    
    #region Http
    public async Task<HttpResponseMessage> PostRequestAsAnonymousAsync(string url, object? data = null)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        var requestData = JsonContent.Create(data ?? new { });
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> PostRequestAsync(
        string url,
        string jwtToken,
        object? data = null,
        Guid? workspaceId = null
    )
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        await SetWorkspaceHeaderAsync(jwtToken, workspaceId);
        var requestData = JsonContent.Create(data ?? new {});
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsAnonymousAsync(
        string url,
        Dictionary<string, string>? urlParams = null
    )
    {
        urlParams ??= new Dictionary<string, string>();
        var queryParams = urlParams.ToDictionary(item => item.Key, item => (string?)item.Value);
        var uri = new Uri(QueryHelpers.AddQueryString(url, queryParams), UriKind.Relative);
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        return await HttpClient.GetAsync(uri);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsync(
        string url,
        string? jwtToken,
        Dictionary<string, string>? urlParams = null,
        Guid? workspaceId = null
    )
    {
        await FlushDbChanges();

        urlParams ??= new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            await SetWorkspaceHeaderAsync(jwtToken, workspaceId);
        }
        else
        {
            HttpClient.DefaultRequestHeaders.Authorization = null;
            HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        }
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "text/json");
        
        var queryParams = urlParams.ToDictionary(item => item.Key, item => (string?)item.Value);
        var uri = new Uri(QueryHelpers.AddQueryString(url, queryParams), UriKind.Relative);
        return await HttpClient.GetAsync(uri);
    }
    
    public async Task<HttpResponseMessage> PostMultipartFormDataRequestAsync(
        string url,
        string? token = null,
        Dictionary<string, object>? data = null,
        IFormFile? file = null
    )
    {
        await FlushDbChanges();

        if (!string.IsNullOrEmpty(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await SetWorkspaceHeaderAsync(token);
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

    private async Task SetWorkspaceHeaderAsync(string jwtToken, Guid? workspaceId = null)
    {
        workspaceId ??= await GetDefaultWorkspaceIdAsync(jwtToken);
        HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        if (workspaceId != null)
        {
            HttpClient.DefaultRequestHeaders.Add(AuthConstants.WorkspaceIdHeaderName, workspaceId.Value.ToString());
        }
    }

    private async Task<Guid?> GetDefaultWorkspaceIdAsync(string jwtToken)
    {
        var userId = _jwtAuthService.GetUserId(jwtToken);
        if (userId == Guid.Empty)
            return null;

        var user = await _userDao.GetById(userId);
        if (user == null)
            return null;

        var workspaces = await _userDao.GetUsersWorkspaces(user);
        return (workspaces.FirstOrDefault(item => !item.IsDefault) ?? workspaces.FirstOrDefault(item => item.IsDefault))?.Id;
    }
    #endregion
    
    #region Uploading

    protected IFormFile CreateFormFile(string fileName = "test.pdf", byte[]? fileBytes = null)
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var stream = new MemoryStream();
            
        var stubsPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        stubsPath = Path.GetDirectoryName(stubsPath);
        stubsPath = Path.Combine(stubsPath!, "stubs");
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
