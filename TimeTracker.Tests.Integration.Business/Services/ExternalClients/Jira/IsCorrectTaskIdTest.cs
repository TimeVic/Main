using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Jira;

public class IsCorrectTaskIdTest : BaseTest
{
    private readonly IJiraClient _client;

    public IsCorrectTaskIdTest() : base(false)
    {
        _client = Scope.Resolve<IJiraClient>();
    }

    [Theory]
    [InlineData("TV-1")]
    public async Task TaskIdShouldBeCorrect(string taskId)
    {
        var timeEntry = new TimeEntryEntity()
        {
            TaskId = taskId
        };
        Assert.True(_client.IsCorrectTaskId(timeEntry));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("SP-21821111111111")]
    [InlineData("#123123888888888")]
    public async Task TaskIdShouldNotBeCorrect(string taskId)
    {
        var timeEntry = new TimeEntryEntity()
        {
            TaskId = taskId
        };
        Assert.False(_client.IsCorrectTaskId(timeEntry));
    }
}
