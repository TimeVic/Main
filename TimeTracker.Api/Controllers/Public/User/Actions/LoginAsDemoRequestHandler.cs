using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
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
        // Client 1 – active with projects
        var client1 = await _clientDao.CreateAsync(ws, "🎯 Acme Corporation");
        var project1 = await _projectDao.CreateAsync(ws, "🐛 Bug Fixes");
        var project2 = await _projectDao.CreateAsync(ws, "🎨 Design System");

        var list1 = await _taskListDao.CreateTaskListAsync(project1, "Sprint 1");
        await _taskDao.AddTaskAsync(list1, user, "Fix login page crash", priority: TaskPriority.High, status: TaskStatus.InProgress);
        await _taskDao.AddTaskAsync(list1, user, "Resolve 500 errors on dashboard", priority: TaskPriority.Medium);
        await _taskDao.AddTaskAsync(list1, user, "Fix broken image uploads", priority: TaskPriority.Low);

        var list2 = await _taskListDao.CreateTaskListAsync(project1, "Sprint 2");
        await _taskDao.AddTaskAsync(list2, user, "Address API timeout issues", priority: TaskPriority.High);
        await _taskDao.AddTaskAsync(list2, user, "Fix mobile layout on iOS", priority: TaskPriority.Medium);

        var list3 = await _taskListDao.CreateTaskListAsync(project2, "🎨 Components");
        await _taskDao.AddTaskAsync(list3, user, "Create Button component", status: TaskStatus.Done);
        await _taskDao.AddTaskAsync(list3, user, "Create Input component", status: TaskStatus.Done);
        await _taskDao.AddTaskAsync(list3, user, "Create Modal component", status: TaskStatus.InProgress);

        // empty task list
        await _taskListDao.CreateTaskListAsync(project2, "Backlog");

        // Client 2 – no projects (intentionally empty)
        await _clientDao.CreateAsync(ws, "Idle Customer");
    }

    private async Task SeedWorkspace2Async(WorkspaceEntity ws, UserEntity user)
    {
        var client1 = await _clientDao.CreateAsync(ws, "🌐 Global Media Group");

        var project1 = await _projectDao.CreateAsync(ws, "🚀 Launch Campaign");
        var list1 = await _taskListDao.CreateTaskListAsync(project1, "Planning");
        await _taskDao.AddTaskAsync(list1, user, "Define campaign goals", status: TaskStatus.Done, priority: TaskPriority.High);
        await _taskDao.AddTaskAsync(list1, user, "Identify target audience", status: TaskStatus.Done);
        await _taskDao.AddTaskAsync(list1, user, "Set KPIs", status: TaskStatus.InProgress);

        var list2 = await _taskListDao.CreateTaskListAsync(project1, "Execution");
        await _taskDao.AddTaskAsync(list2, user, "Create landing page copy");
        await _taskDao.AddTaskAsync(list2, user, "Design social media assets", priority: TaskPriority.Medium);

        var project2 = await _projectDao.CreateAsync(ws, "📊 Analytics Dashboard");
        var list3 = await _taskListDao.CreateTaskListAsync(project2, "Features");
        await _taskDao.AddTaskAsync(list3, user, "Implement funnel report");
        await _taskDao.AddTaskAsync(list3, user, "Add export to CSV");

        // empty project
        await _projectDao.CreateAsync(ws, "Future Ideas");

        var client2 = await _clientDao.CreateAsync(ws, "🏆 Top Client Inc");
        var project3 = await _projectDao.CreateAsync(ws, "🔒 Security Audit");
        var list4 = await _taskListDao.CreateTaskListAsync(project3, "Audit Checklist");
        await _taskDao.AddTaskAsync(list4, user, "Review authentication flows", priority: TaskPriority.High);
        await _taskDao.AddTaskAsync(list4, user, "Check OWASP top 10", priority: TaskPriority.High);
        await _taskDao.AddTaskAsync(list4, user, "Generate penetration test report");
    }
}




