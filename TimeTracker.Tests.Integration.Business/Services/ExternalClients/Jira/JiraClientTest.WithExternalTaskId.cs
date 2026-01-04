using TimeTracker.Business.Extensions;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Jira;

public partial class SendNewTimeEntityTest : BaseTest
{
    // [Fact]
    public async Task ShouldUseTaskIdFromTaskIfExists()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.ExternalTaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        
        var date = DateTime.UtcNow.Date.ToDateOnly();
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date.ToDateOnly(),
            DateTime.UtcNow.TimeOfDay,
            true,
            internalTask: task
        );
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.TimeOfDay, date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _client.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.NotEmpty(actualResponse.Id);

        activeEntry.JiraId = long.Parse(actualResponse.Id);
        var isDeleted = await _client.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }
}
