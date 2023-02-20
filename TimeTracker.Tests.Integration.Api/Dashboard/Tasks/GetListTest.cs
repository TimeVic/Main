using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

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
            TaskListId = _taskList.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Title);
            Assert.NotNull(item.TaskList);
            Assert.NotEmpty(item.Description);
            Assert.Equal(_taskList.Id, item.TaskList.Id);
        });
        Assert.Contains(actualDto.Items, item =>
        {
            return item.Attachments.Any() && !string.IsNullOrEmpty(item.Attachments.First().Url);
        });
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
            Page = 1,
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
    public async Task ShouldFilterByIsDone()
    {
        var expectedCounter = 7;
        var otherTasks = await _taskSeeder.CreateSeveralAsync(_taskList, 4);
        foreach (var task in otherTasks)
        {
            task.IsDone = false;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        foreach (var task in tasks)
        {
            task.IsDone = true;
            await DbSessionProvider.CurrentSession.SaveAsync(task);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Page = 1,
            Filter = new GetListFilterRequest()
            {
                IsDone = true
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
            Page = 1,
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
            Page = 1,
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
            Page = 1,
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
            TaskListId = _taskList.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Contains(actualDto.Items, item => item.Tags.Any());
    }
}
