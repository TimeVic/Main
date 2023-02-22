using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.TimeEntry;

public partial class StartTest
{
    [Fact]
    public async Task ShouldStartWithProvidedInternalTaskId()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectDao.CreateAsync(_defaultWorkspace, "Test project");
        await CommitDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            IsBillable = fakeTimeEntry.IsBillable,
            Date = DateTime.UtcNow.Date,
            StartTime = TimeSpan.FromSeconds(1),
            TaskId = fakeTimeEntry.TaskId,
            InternalTaskId = task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.True(actualDto.Id > 0);
        Assert.Null(actualDto.TaskId);
        Assert.NotNull(actualDto.Task);
        Assert.True(actualDto.Task.Id > 0);
    }
}
