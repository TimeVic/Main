using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public partial class UpdateTest
{
    [Fact]
    public async Task ShouldUpdateAndAddNewTags()
    {
        var expectedTags = 3;
        var newTags = await _tagSeeder.CreateSeveralAsync(_workspace, expectedTags);
        
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            NotificationTime = expectedTask.NotificationTime,
            IsDone = expectedTask.IsDone,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            TagIds = newTags.Select(item => item.Id).ToList()
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.Equal(_task.Id, actualData.Id);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.IsDone, actualData.IsDone);
        Assert.Equal(expectedTask.IsArchived, actualData.IsArchived);
        Assert.Equal(expectedTask.ExternalTaskId, actualData.ExternalTaskId);
        Assert.Equal(expectedTags, actualData.Tags.Count);
    }
    
    [Fact]
    public async Task ShouldRemoveOldTestsAndAddNew()
    {
        var expectedTags = 3;
        var oldTags = await _tagSeeder.CreateSeveralAsync(_workspace, 2);
        var newTags = await _tagSeeder.CreateSeveralAsync(_workspace, expectedTags);
        foreach (var tag in oldTags)
        {
            _task.Tags.Add(tag);    
        }
        await CommitDbChanges();
        
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            NotificationTime = expectedTask.NotificationTime,
            IsDone = expectedTask.IsDone,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            TagIds = newTags.Select(item => item.Id).ToList()
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.Equal(_task.Id, actualData.Id);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.IsDone, actualData.IsDone);
        Assert.Equal(expectedTask.IsArchived, actualData.IsArchived);
        Assert.Equal(expectedTask.ExternalTaskId, actualData.ExternalTaskId);
        Assert.Equal(expectedTags, actualData.Tags.Count);
    }
}
