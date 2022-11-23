using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Notifications.Services;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;

namespace TimeTracker.Tests.Integration.Api.Core;

public class BaseTest: IClassFixture<ApiCustomWebApplicationFactory>, IDisposable
{
    protected readonly ApiCustomWebApplicationFactory _factory;
    
    protected readonly IServiceProvider ServiceProvider;
    protected readonly HttpClient HttpClient;
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly IUserSeeder UserSeeder;
    protected readonly IDataFactory<UserEntity> UserFactory;
    protected readonly EmailSendingServiceMock EmailSendingServiceMock;

    public BaseTest(ApiCustomWebApplicationFactory factory)
    {
        _factory = factory;
        HttpClient = _factory.CreateClient();
        
        DbSessionProvider = _factory.Services.GetRequiredService<IDbSessionProvider>();
        UserSeeder = _factory.Services.GetRequiredService<IUserSeeder>();
        UserFactory = _factory.Services.GetRequiredService<IDataFactory<UserEntity>>();
        EmailSendingServiceMock = _factory.Services.GetRequiredService<IEmailSendingService>() as EmailSendingServiceMock;
        ServiceProvider = _factory.Services;
    }
    
    public void Dispose()
    {
        DbSessionProvider.PerformCommitAsync().Wait();
        GC.SuppressFinalize(this);
    }

    protected async Task CommitDbChanges()
    { 
        await DbSessionProvider.PerformCommitAsync();
        DbSessionProvider.CurrentSession.Clear();
    }
    
    #region Http
    public async Task<HttpResponseMessage> PostRequestAsAnonymousAsync(string url, object data = null)
    {
        await CommitDbChanges();

        var requestData = JsonContent.Create(data ?? new { });
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> PostRequestAsync(string url, string jwtToken,  object data = null)
    {
        await CommitDbChanges();

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
        await CommitDbChanges();

        return await HttpClient.GetAsync(uri);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsync(string url, string jwtToken,  object data = null)
    {
        await CommitDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "text/json");
        return await HttpClient.GetAsync(url);
    }
    #endregion
}
