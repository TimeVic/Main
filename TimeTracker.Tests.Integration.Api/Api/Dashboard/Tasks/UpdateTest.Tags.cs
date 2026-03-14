using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Extensions;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

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
            StartTime = expectedTask.StartTime,
            EndTime = expectedTask.EndTime,
            Status = expectedTask.Status,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            TagIds = newTags.Select(item => item.Id).ToList()
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.Equal(_task.TaskId, actualData.TaskId);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.Status, actualData.Status);
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
        await FlushDbChanges();
        
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            StartTime = expectedTask.StartTime,
            EndTime = expectedTask.EndTime,
            Status = expectedTask.Status,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            TagIds = newTags.Select(item => item.Id).ToList()
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.Equal(_task.TaskId, actualData.TaskId);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.Status, actualData.Status);
        Assert.Equal(expectedTask.IsArchived, actualData.IsArchived);
        Assert.Equal(expectedTask.ExternalTaskId, actualData.ExternalTaskId);
        Assert.Equal(expectedTags, actualData.Tags.Count);
    }
}
