using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Redmine;

public class IsCorrectTaskIdTest : BaseTest
{
    private readonly IRedmineClient _redmineClient;
    private readonly string _securityKey;

    private readonly string _teamId;
    private readonly string _taskId;

    public IsCorrectTaskIdTest() : base(false)
    {
        _redmineClient = Scope.Resolve<IRedmineClient>();
    }

    [Theory]
    [InlineData("12")]
    [InlineData("123456")]
    public async Task TaskIdShouldBeCorrect(string taskId)
    {
        var timeEntry = new TimeEntryEntity()
        {
            TaskId = taskId
        };
        Assert.True(_redmineClient.IsCorrectTaskId(timeEntry));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("12w")]
    [InlineData("123456-")]
    [InlineData("123a456")]
    public async Task TaskIdShouldNotBeCorrect(string taskId)
    {
        var timeEntry = new TimeEntryEntity()
        {
            TaskId = taskId
        };
        Assert.False(_redmineClient.IsCorrectTaskId(timeEntry));
    }
}
