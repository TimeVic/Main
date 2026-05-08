using Autofac;
using NHibernate;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IProjectSeeder _projectSeeder;
    
    private readonly WorkspaceEntity _workspace;
    private readonly UserEntity _user;
    private readonly IUserDao _userDao;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _workspaceDao.CreateWorkspaceAsync(_user, "Test").Result;
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter);

        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
        
        Assert.All(actualList.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.True(item.CreatedAt > DateTime.MinValue);
            Assert.True(item.EndTime > DateTime.MinValue);
            Assert.NotNull(item.Project);
        });
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherNamespaces()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        var user2Workspace = _userDao.GetUsersWorkspaces(user2, MembershipAccessType.Owner).Result.First();
        await _timeEntrySeeder.CreateSeveralAsync(user2Workspace, user2, 15);
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldSortList()
    {
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);

        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);

        var actualFirst = actualList.Items.First();
        var actualLast = actualList.Items.Last();
        Assert.True(actualFirst.StartTime > actualLast.StartTime);
    }

    [Fact]
    public async Task GetListGroupedByDayShouldNotSplitSingleDayBetweenPages()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var daysInPage = GlobalConstants.TimeEntryGroupedByDayPageSize;
        var baseDay = DateTime.UtcNow.Date;
        var boundaryDay = baseDay.AddDays(-(daysInPage - 1));

        for (var i = 0; i < daysInPage + 2; i++)
        {
            var startTime = baseDay.AddDays(-i).AddHours(10);
            await _timeEntryDao.SetAsync(
                _user,
                _workspace,
                new TimeEntryCreationDto()
                {
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1),
                    IsBillable = true,
                    HourlyRate = 10
                },
                project
            );
        }

        for (var i = 0; i < 3; i++)
        {
            var startTime = boundaryDay.AddHours(12 + i);
            await _timeEntryDao.SetAsync(
                _user,
                _workspace,
                new TimeEntryCreationDto()
                {
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(30),
                    IsBillable = true,
                    HourlyRate = 12
                },
                project
            );
        }

        await FlushDbChanges();
        var firstPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 1);
        var secondPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 2);

        var boundaryItemsOnFirstPage = firstPage.Items
            .Where(item => item.StartTime.Date == boundaryDay)
            .ToList();
        var boundaryItemsOnSecondPage = secondPage.Items
            .Where(item => item.StartTime.Date == boundaryDay)
            .ToList();

        Assert.True(boundaryItemsOnFirstPage.Count >= 4);
        Assert.Empty(boundaryItemsOnSecondPage);
    }

    [Fact]
    public async Task GetListGroupedByDayShouldReturnAllItemsForSingleDay()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var day = DateTime.UtcNow.Date.AddDays(-3);
        var expectedCount = GlobalConstants.ListPageSize + 7;

        for (var i = 0; i < expectedCount; i++)
        {
            var startTime = day.AddMinutes(i);
            await _timeEntryDao.SetAsync(
                _user,
                _workspace,
                new TimeEntryCreationDto
                {
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(15),
                    IsBillable = true,
                    HourlyRate = 15
                },
                project
            );
        }

        await FlushDbChanges();
        var firstPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 1);
        var secondPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 2);

        Assert.Equal(expectedCount, firstPage.Items.Count);
        Assert.All(firstPage.Items, item => Assert.Equal(day, item.StartTime.Date));
        Assert.Empty(secondPage.Items);
    }

    [Fact]
    public async Task GetListGroupedByDayShouldLoadNavigationProperties()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _timeEntryDao.SetAsync(
            _user,
            _workspace,
            new TimeEntryCreationDto
            {
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                IsBillable = true,
                HourlyRate = 10
            },
            project
        );

        await FlushDbChanges();
        var page = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 1);

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item =>
        {
            Assert.True(NHibernateUtil.IsInitialized(item.User));
            Assert.True(NHibernateUtil.IsInitialized(item.Project));
            if (item.Project != null)
            {
                Assert.True(NHibernateUtil.IsInitialized(item.Project.Client));
            }
            Assert.True(NHibernateUtil.IsInitialized(item.Task));
        });
    }

    [Fact]
    public async Task GetListGroupedByDayShouldReturnDistinctDaysAsTotalCount()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var baseDay = DateTime.UtcNow.Date;

        for (var i = 0; i < 5; i++)
        {
            var startTime = baseDay.AddDays(-i).AddHours(9);
            await _timeEntryDao.SetAsync(
                _user,
                _workspace,
                new TimeEntryCreationDto
                {
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1),
                    IsBillable = true,
                    HourlyRate = 10
                },
                project
            );

            await _timeEntryDao.SetAsync(
                _user,
                _workspace,
                new TimeEntryCreationDto
                {
                    StartTime = startTime.AddHours(2),
                    EndTime = startTime.AddHours(3),
                    IsBillable = true,
                    HourlyRate = 10
                },
                project
            );
        }

        await FlushDbChanges();
        var firstPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 1);
        var secondPage = await _timeEntryDao.GetListGroupedByDayAsync(_workspace, _user, 2);

        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(5, secondPage.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByClient()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_workspace)).First();
        Assert.NotNull(expectedProject.Client);
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter, expectedProject);

        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            ClientId = expectedProject.Client.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByProject()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_workspace)).First();
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter, expectedProject);

        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            ProjectId = expectedProject.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBillable()
    {
        var expectedCounter = 7;
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9))
        {
            timeEntryEntity.IsBillable = false;
        }
        
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter))
        {
            timeEntryEntity.IsBillable = true;
        }
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            IsBillable = true
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBySearchString()
    {
        var expectedDescription = "Some fake desc";
        var user = await _userSeeder.CreateActivatedAsync();
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, user, 9)).First();
        expectedEntry.Description = expectedDescription;
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            Search = "FAKE"
        });
        Assert.Equal(1, actualList.TotalCount);
        Assert.Equal(expectedEntry.Id, actualList.Items.First().Id);
    }
    
    [Fact]
    public async Task ShouldFilterByMember()
    {
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);
        
        var expectedUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            access: MembershipAccessType.User
        );
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, expectedUser, 3);
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            filter: new FilterDataDto()
            {
                 MemberId = expectedUser.Id
            }
        );
        
        Assert.Equal(3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.User.Id == expectedUser.Id);
        });
    }
    
    [Fact]
    public async Task ShouldFilterByDateFrom()
    {
        var dateFrom = DateTime.UtcNow.AddDays(-5).Date;
        
        var expectedEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);
        foreach (var entry in expectedEntries)
        {
            entry.StartTime = DateTime.UtcNow.AddDays(-5);
        }
        var notExpectedEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 2);
        foreach (var entry in notExpectedEntries)
        {
            entry.StartTime = DateTime.UtcNow.AddDays(-6);
        }
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            filter: new FilterDataDto()
            {
                DateFrom = dateFrom
            }
        );
        
        Assert.Equal(3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.StartTime.Date >= dateFrom);
        });
    }
    
    [Fact]
    public async Task ShouldFilterByDateTo()
    {
        var dateTo = DateTime.UtcNow.Date;
        
        var expectedEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);
        foreach (var entry in expectedEntries)
        {
            entry.StartTime = DateTime.UtcNow.AddDays(-1);
        }
        expectedEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);
        foreach (var entry in expectedEntries)
        {
            entry.StartTime = DateTime.UtcNow;
        }
        var notExpectedEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 2);
        foreach (var entry in notExpectedEntries)
        {
            entry.StartTime = DateTime.UtcNow.AddDays(1);
        }
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            filter: new FilterDataDto()
            {
                DateTo = dateTo.EndOfDay()
            }
        );
        
        Assert.Equal(6, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.StartTime.Date <= dateTo);
        });
    }
    
    [Fact]
    public async Task ShouldReturnSharedProjectAndOwnTimeEntriesForUserWithRoleUser()
    {
        var accessType = MembershipAccessType.User;
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, 4);
        foreach (var project in projects)
        {
            await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3, project);
        }
        var expectedProject1 = projects.First();
        var expectedProject2 = projects.Last();
        var expectedUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            access: accessType,
            projects: new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject1 },
                new ProjectAccessModel() { Project = expectedProject2 }
            }
        );
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, expectedUser, 3);
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            user: expectedUser,
            accessType: accessType
        );
        
        Assert.Equal(9, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.NotNull(item.Project);
            Assert.True(
                item.Project.Id == expectedProject1.Id 
                || item.Project.Id == expectedProject2.Id
                || item.User.Id == expectedUser.Id
            );
        });
    }
    
    [Fact]
    public async Task ShouldReturnAllTimeEntriesIfUserWithRoleManager()
    {
        var accessType = MembershipAccessType.Manager;
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, 4);
        foreach (var project in projects)
        {
            await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3, project);
        }
        var expectedUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            access: accessType
        );
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, expectedUser, 3);
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            user: expectedUser,
            accessType: accessType
        );
        
        Assert.Equal(4 * 3 + 3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.User.Id == expectedUser.Id || item.User.Id == _user.Id);
        });
    }
    
    [Fact]
    public async Task ShouldNotReceiveMarkedToDeleteTimeEntries()
    {
        var expectedCounter = 7;
        var entries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter);
        var markedToDelete = entries.First();
        markedToDelete.IsMarkedToDelete = true;
        
        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(expectedCounter - 1, actualList.TotalCount);
    }
}
