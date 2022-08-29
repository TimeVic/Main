using Microsoft.AspNetCore.Mvc.Testing;

namespace TimeTracker.Tests.Integration.Api.Core;

public class BaseTest
{
    protected readonly HttpClient HttpClient;

    public BaseTest()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // ... Configure test services
            });

        HttpClient = application.CreateClient();
    }
}
