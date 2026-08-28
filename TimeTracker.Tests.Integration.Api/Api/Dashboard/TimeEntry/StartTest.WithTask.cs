using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Extensions;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public partial class StartTest
{
    [Fact]
    public async Task ShouldStartWithProvidedInternalTaskId()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        await FlushDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            IsBillable = fakeTimeEntry.IsBillable,
            InternalTaskId = task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = (await response.GetJsonDataAsync<StartResponse>()).ActiveTimeEntry;
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.NotNull(actualDto.Task);
        Assert.True(actualDto.Task.TaskId > 0);
    }
    
}
