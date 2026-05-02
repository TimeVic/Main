using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Security;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class LoginAsDemoRequestHandler : IAsyncRequestHandler<LoginAsDemoRequest, LoginResponseDto>
{
    private static readonly TimeSpan DemoUserMaxAge = TimeSpan.FromDays(7);

    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IClientDao _clientDao;
    private readonly IProjectDao _projectDao;
    private readonly ITaskListDao _taskListDao;
    private readonly ITaskDao _taskDao;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ICurrencyDao _currencyDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IPasswordService _passwordService;

    public LoginAsDemoRequestHandler(
        IMapper mapper,
        IAuthorizationService authorizationService,
        IUserDao userDao,
        IWorkspaceDao workspaceDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskListDao taskListDao,
        ITaskDao taskDao,
        IDbSessionProvider sessionProvider,
        ICurrencyDao currencyDao,
        IWorkspaceAccessService workspaceAccessService,
        IPasswordService passwordService
    )
    {
        _mapper = mapper;
        _authorizationService = authorizationService;
        _userDao = userDao;
        _workspaceDao = workspaceDao;
        _clientDao = clientDao;
        _projectDao = projectDao;
        _taskListDao = taskListDao;
        _taskDao = taskDao;
        _sessionProvider = sessionProvider;
        _currencyDao = currencyDao;
        _workspaceAccessService = workspaceAccessService;
        _passwordService = passwordService;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginAsDemoRequest request)
    {
        var demoUser = await GetOrCreateDemoUserAsync();

        var loginResponse = await _authorizationService.Login(demoUser);
        var userDto = _mapper.Map<UserDto>(loginResponse.User);
        var defaultWorkspace = await _userDao.GetDefaultWorkspace(demoUser);
        userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);

        return new LoginResponseDto
        {
            JwtToken = loginResponse.JwtToken,
            AccessToken = loginResponse.AccessToken,
            User = userDto
        };
    }

    private async Task<UserEntity> GetOrCreateDemoUserAsync()
    {
        var existingUser = await _userDao.GetLastDemoUserAsync();
        if (existingUser != null && DateTime.UtcNow - existingUser.CreatedAt < DemoUserMaxAge)
        {
            return existingUser;
        }

        var email = DemoAccountConstants.GenerateEmail();
        var user = await _userDao.CreatePendingUser(email);
        user.UserName = "jоhn_deere";
        user.VerificationTime = DateTime.UtcNow;
        user.VerificationToken = null;
        _passwordService.SetUserPassword(user, Guid.NewGuid().ToString());

        await SeedDemoDataAsync(user);

        return user;
    }

    private async Task SeedDemoDataAsync(UserEntity user)
    {
        var currencies = await _currencyDao.GetAll();
        var usd = currencies.FirstOrDefault(c => c.Code == "USD") ?? await _currencyDao.GetDefault();
        var eur = currencies.FirstOrDefault(c => c.Code == "EUR") ?? usd;
        var gbp = currencies.FirstOrDefault(c => c.Code == "GBP") ?? usd;

        // Workspace 1 – main workspace (default), UTC / USD
        var ws1 = await _workspaceDao.CreateWorkspaceAsync(user, "🚀 My Startup", isDefault: true);
        await _workspaceAccessService.ShareAccessAsync(ws1, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws1, ws1.Name, usd, "UTC", "Main demo workspace");
        await SeedWorkspace1Async(ws1, user);

        // Workspace 2 – US timezone / EUR
        var ws2 = await _workspaceDao.CreateWorkspaceAsync(user, "🌍 European Office");
        await _workspaceAccessService.ShareAccessAsync(ws2, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws2, ws2.Name, eur, "America/New_York", null);
        await SeedWorkspace2Async(ws2, user);

        // Workspace 3 – Berlin / GBP (intentionally sparse)
        var ws3 = await _workspaceDao.CreateWorkspaceAsync(user, "🏢 Berlin Hub");
        await _workspaceAccessService.ShareAccessAsync(ws3, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws3, ws3.Name, gbp, "Europe/Berlin", null);
        // just a few clients, no projects
        await _clientDao.CreateAsync(ws3, "Prospect Corp");
        await _clientDao.CreateAsync(ws3, "🤝 Partnership Ltd");
    }

    private async Task SeedWorkspace1Async(WorkspaceEntity ws, UserEntity user)
    {
        var tasks = new List<TaskEntity>();

        var acmeClient = await _clientDao.CreateAsync(ws, "🎯 Acme Corporation");
        var northstarClient = await _clientDao.CreateAsync(ws, "🧭 Northstar Labs");
        var brightAppsClient = await _clientDao.CreateAsync(ws, "📱 Bright Apps");
        var project1 = await CreateDemoProjectAsync(ws, "🐛 Bug Fixes", 95, acmeClient);
        var project2 = await CreateDemoProjectAsync(ws, "🎨 Design System", 110, northstarClient);
        var project3 = await CreateDemoProjectAsync(ws, "⚙️ Platform Maintenance", 85, acmeClient);
        var project4 = await CreateDemoProjectAsync(ws, "📱 Mobile App", 105, brightAppsClient);

        var list1 = await _taskListDao.CreateTaskListAsync(project1, "Sprint 1");
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Fix login page crash", priority: TaskPriority.High, status: TaskStatus.InProgress));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Resolve 500 errors on dashboard", priority: TaskPriority.Medium));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Fix broken image uploads", priority: TaskPriority.Low));

        var list2 = await _taskListDao.CreateTaskListAsync(project1, "Sprint 2");
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Address API timeout issues", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Fix mobile layout on iOS", priority: TaskPriority.Medium));

        var list3 = await _taskListDao.CreateTaskListAsync(project2, "🎨 Components");
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Create Button component", status: TaskStatus.Done));
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Create Input component", status: TaskStatus.Done));
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Create Modal component", status: TaskStatus.InProgress));

        // empty task list
        await _taskListDao.CreateTaskListAsync(project2, "Backlog");

        var list4 = await _taskListDao.CreateTaskListAsync(project3, "Maintenance");
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Upgrade background worker logs", priority: TaskPriority.Medium));
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Clean stale demo accounts", status: TaskStatus.InProgress));

        var list5 = await _taskListDao.CreateTaskListAsync(project4, "Release 1.4");
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Polish timer widget", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Add offline sync banner", priority: TaskPriority.Medium));

        await _clientDao.CreateAsync(ws, "📦 Supply Chain Co");
        await _clientDao.CreateAsync(ws, "Idle Customer");

        await SeedTimeEntriesAsync(ws, user, tasks);
    }

    private async Task SeedWorkspace2Async(WorkspaceEntity ws, UserEntity user)
    {
        var tasks = new List<TaskEntity>();

        var mediaClient = await _clientDao.CreateAsync(ws, "🌐 Global Media Group");
        var insightClient = await _clientDao.CreateAsync(ws, "📈 Insight Partners");
        var topClient = await _clientDao.CreateAsync(ws, "🏆 Top Client Inc");
        var qualityClient = await _clientDao.CreateAsync(ws, "🧪 Quality Works");

        var project1 = await CreateDemoProjectAsync(ws, "🚀 Launch Campaign", 120, mediaClient);
        var list1 = await _taskListDao.CreateTaskListAsync(project1, "Planning");
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Define campaign goals", status: TaskStatus.Done, priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Identify target audience", status: TaskStatus.Done));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Set KPIs", status: TaskStatus.InProgress));

        var list2 = await _taskListDao.CreateTaskListAsync(project1, "Execution");
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Create landing page copy"));
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Design social media assets", priority: TaskPriority.Medium));

        var project2 = await CreateDemoProjectAsync(ws, "📊 Analytics Dashboard", 115, insightClient);
        var list3 = await _taskListDao.CreateTaskListAsync(project2, "Features");
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Implement funnel report"));
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Add export to CSV"));

        // empty project
        await _projectDao.CreateAsync(ws, "Future Ideas");

        var project3 = await CreateDemoProjectAsync(ws, "🔒 Security Audit", 140, topClient);
        var list4 = await _taskListDao.CreateTaskListAsync(project3, "Audit Checklist");
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Review authentication flows", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Check OWASP top 10", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Generate penetration test report"));

        var project4 = await CreateDemoProjectAsync(ws, "🧪 QA Automation", 100, qualityClient);
        var list5 = await _taskListDao.CreateTaskListAsync(project4, "Regression");
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Stabilize smoke tests", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Add reporting screenshots", priority: TaskPriority.Medium));

        await SeedTimeEntriesAsync(ws, user, tasks);
    }

    private async Task<ProjectEntity> CreateDemoProjectAsync(
        WorkspaceEntity workspace,
        string name,
        decimal? defaultHourlyRate,
        ClientEntity? client = null
    )
    {
        var project = await _projectDao.CreateAsync(workspace, name);
        project.DefaultHourlyRate = defaultHourlyRate;
        project.IsBillableByDefault = defaultHourlyRate.HasValue;
        project.SetClient(client);
        return project;
    }

    private async Task SeedTimeEntriesAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        IReadOnlyCollection<TaskEntity> tasks
    )
    {
        var index = 0;
        foreach (var task in tasks)
        {
            for (var entryIndex = 0; entryIndex < 2; entryIndex++)
            {
                var dayOffset = -6 + (index + entryIndex) % 7;
                var startTime = DateTime.UtcNow.Date
                    .AddDays(dayOffset)
                    .AddHours(9 + (index % 6))
                    .AddMinutes(entryIndex * 90);
                var duration = TimeSpan.FromMinutes(45 + (index % 4) * 30 + entryIndex * 15);
                var project = task.TaskList.Project;
                var timeEntry = new TimeEntryEntity
                {
                    Workspace = workspace,
                    User = user,
                    Project = project,
                    Task = task,
                    Description = task.Title,
                    IsBillable = project.IsBillableByDefault,
                    HourlyRate = project.DefaultHourlyRate,
                    StartTime = startTime,
                    EndTime = startTime.Add(duration),
                    TimeZone = workspace.TimeZone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
            }

            index++;
        }
    }
}
