using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Connection;

namespace TimeTracker.Tests.Integration.Api.Core;

public class BaseTest
{
    protected readonly HttpClient HttpClient;
    protected readonly IDbSessionProvider DbSessionProvider;

    public BaseTest()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // ... Configure test services
            });
        DbSessionProvider = application.Services.GetRequiredService<IDbSessionProvider>();

        HttpClient = application.CreateClient();
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
        
    public async Task<HttpResponseMessage> GetRequestAsAnonymousAsync(string url)
    {
        await CommitDbChanges();

        return await HttpClient.GetAsync(url);
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
