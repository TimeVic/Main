using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api;

public class UnitTest1: BaseTest
{
    [Fact]
    public async Task Test1()
    {
        var response = await HttpClient.GetAsync("/sum?n1=10&n2=6");
        var stringResult = await response.Content.ReadAsStringAsync();
        var intResult = int.Parse(stringResult);

        Assert.Equal(16, intResult);
    }
}
