using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public class IsCorrectTaskIdTest : BaseTest
{
    private readonly IClickUpClient _сlickUpClient;
    private readonly string _securityKey;

    private readonly string _teamId;
    private readonly string _taskId;

    public IsCorrectTaskIdTest() : base(false)
    {
        _сlickUpClient = Scope.Resolve<IClickUpClient>();
    }

    [Theory]
    [InlineData("SP-2182")]
    [InlineData("#30cjgez")]
    public async Task TaskIdShouldBeCorrect(string taskId)
    {
        var timeEntry = new TimeEntryEntity()
        {
            TaskId = taskId
        };
        Assert.True(_сlickUpClient.IsCorrectTaskId(timeEntry));
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
        Assert.False(_сlickUpClient.IsCorrectTaskId(timeEntry));
    }
}
