using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/get-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly TaskListEntity _taskList;
    
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IFileStorage _fileStorage;
    private readonly ITagSeeder _tagSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_defaultWorkspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        await _fileStorage.PutFileAsync(tasks.First(), CreateFormFile(), StoredFileType.Attachment);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        Assert.NotNull(actualDto.TaskList);
        Assert.Equal(_taskList.Id, actualDto.TaskList.Id);
        Assert.Equal(_defaultWorkspace.Id, actualDto.TaskList.WorkspaceId);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.TaskId > 0);
            Assert.NotEmpty(item.Title);
            Assert.NotNull(item.TaskList);
            Assert.NotEmpty(item.Description!);
            Assert.Equal(TaskPriority.Medium, item.Priority);
            Assert.Equal(_taskList.Id, item.TaskList.Id);
            Assert.Equal(_defaultWorkspace.Id, item.TaskList.WorkspaceId);
        });
    }

    [Fact]
    public async Task ShouldNotReceiveListIfWorkspaceDoesNotMatch()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest
        {
            TaskListId = _taskList.Id
        }, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldReceiveTrackedDuration()
    {
        var now = DateTime.UtcNow;
        var task = await _taskSeeder.CreateAsync(_taskList, _user);
        var timeEntry = await _timeEntryDao.SetAsync(
            _user,
            _defaultWorkspace,
            new TimeEntryCreationDto
            {
                Description = "Tracked task time",
                StartTime = now.AddHours(-2),
                EndTime = now.AddHours(-1),
                IsBillable = false
            },
            _project
        );
        timeEntry.Task = task;
        await DbSessionProvider.CurrentSession.SaveAsync(timeEntry);

        await FlushDbChanges(true);

        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        var actualTask = actualDto.Items.Single(item => item.Id == task.Id);
        Assert.Equal(TimeSpan.FromHours(1), actualTask.TrackedDuration);
    }

    [Fact]
    public async Task ShouldSortByPositionIndex()
    {
        var firstTask = await _taskSeeder.CreateAsync(_taskList, _user);
        var secondTask = await _taskSeeder.CreateAsync(_taskList, _user);
        var thirdTask = await _taskSeeder.CreateAsync(_taskList, _user);

        firstTask.PositionIndex = 2;
        secondTask.PositionIndex = 0;
        thirdTask.PositionIndex = 1;
        await DbSessionProvider.CurrentSession.SaveAsync(firstTask);
        await DbSessionProvider.CurrentSession.SaveAsync(secondTask);
        await DbSessionProvider.CurrentSession.SaveAsync(thirdTask);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        var items = actualDto.Items.ToList();
        Assert.Equal(3, actualDto.TotalCount);
        Assert.Equal(secondTask.Id, items[0].Id);
        Assert.Equal(thirdTask.Id, items[1].Id);
        Assert.Equal(firstTask.Id, items[2].Id);
    }

    [Fact]
    public async Task ShouldKeepOrderAfterTaskUpdate()
    {
        var firstTask = await _taskSeeder.CreateAsync(_taskList, _user);
        var secondTask = await _taskSeeder.CreateAsync(_taskList, _user);

        firstTask.PositionIndex = 0;
        secondTask.PositionIndex = 1;
        await DbSessionProvider.CurrentSession.SaveAsync(firstTask);
        await DbSessionProvider.CurrentSession.SaveAsync(secondTask);

        var updateResponse = await PostRequestAsync(Url.Replace("get-list", "update"), _jwtToken, new UpdateRequest
        {
            TaskId = secondTask.Id,
            TaskListId = _taskList.Id,
            Title = secondTask.Title,
            Description = "Updated description",
            StartTime = secondTask.StartTime,
            EndTime = secondTask.EndTime,
            Status = secondTask.Status,
            Priority = secondTask.Priority,
            IsArchived = secondTask.IsArchived,
            UserId = secondTask.User.Id
        });
        updateResponse.EnsureSuccessStatusCode();

        var listResponse = await PostRequestAsync(Url, _jwtToken, new GetListRequest
        {
            TaskListId = _taskList.Id
        });
        listResponse.EnsureSuccessStatusCode();

        var actualDto = await listResponse.GetJsonDataAsync<GetListResponse>();
        var items = actualDto.Items.ToList();
        Assert.Equal(firstTask.Id, items[0].Id);
        Assert.Equal(secondTask.Id, items[1].Id);
    }
    
    [Fact]
    public async Task ShouldFilterByAssignee()
    {
        var user2 = await UserSeeder.CreateActivatedAndShareAsync(_defaultWorkspace);
        var expectedCounter = 7;
        await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter, user2);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                AssignedUserId = user2.Id
            }
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByStatus()
    {
        var expectedCounter = 7;
        var otherTasks = await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        foreach (var task in otherTasks)
        {
            task.Status = TaskStatus.Done;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        foreach (var task in tasks)
        {
            task.Status = TaskStatus.InProgress;;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                Status = TaskStatus.InProgress
            }
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByIsArchived()
    {
        var expectedCounter = 7;
        var otherTasks = await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        foreach (var task in otherTasks)
        {
            task.IsArchived = false;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        foreach (var task in tasks)
        {
            task.IsArchived = true;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                IsArchived = true
            }
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByIsArchivedIsNull()
    {
        var otherTasks = await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        foreach (var task in otherTasks)
        {
            task.IsArchived = false;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, 7);
        foreach (var task in tasks)
        {
            task.IsArchived = true;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                IsArchived = null
            }
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(11, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBySearchStringInTitle()
    {
        var expectedCounter = 7;
        var expectedSearchString = "Some 123 string";
        await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        foreach (var task in tasks)
        {
            task.Title = $"{task.Title} {expectedSearchString.ToLower()} ";
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                SearchString = expectedSearchString
            }
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBySearchStringInDescription()
    {
        var expectedCounter = 7;
        var expectedSearchString = "Some 123 string";
        await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        foreach (var task in tasks)
        {
            task.Description = $"{task.Description} {expectedSearchString.ToLower()} ";
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Filter = new GetListFilterRequest()
            {
                SearchString = expectedSearchString
            }
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
    
    [Fact]
    public async Task ShouldReceiveListWithTags()
    {
        var tags = await _tagSeeder.CreateSeveralAsync(_defaultWorkspace, 2);
        
        var expectedCounter = 15;
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        var task = tasks.First();
        foreach (var tag in tags)
        {
            task.Tags.Add(tag);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Contains(actualDto.Items, item => item.Tags.Any());
    }
    
}
