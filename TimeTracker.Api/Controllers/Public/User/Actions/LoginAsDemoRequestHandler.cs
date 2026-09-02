using Api.Requests.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class LoginAsDemoRequestHandler : IAsyncRequestHandler<LoginAsDemoRequest, LoginResponseDto>
{
    private static readonly TimeSpan DemoUserMaxAge = TimeSpan.FromDays(7);

    private readonly IAuthorizationService _authorizationService;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IClientDao _clientDao;
    private readonly IProjectDao _projectDao;
    private readonly ITaskListDao _taskListDao;
    private readonly ITaskDao _taskDao;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly ICurrencyDao _currencyDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IPasswordService _passwordService;
    private readonly IHttpCookiesService _cookiesService;
    private readonly IUserDtoBuilder _userDtoBuilder;
    private readonly ITimeEntryService _timeEntryService;
    private readonly ITimeEntryApprovalService _timeEntryApprovalService;

    public LoginAsDemoRequestHandler(
        IAuthorizationService authorizationService,
        IUserDao userDao,
        IWorkspaceDao workspaceDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskListDao taskListDao,
        ITaskDao taskDao,
        IDbSessionProvider sessionProvider,
        IMemberPaymentDao paymentDao,
        ICurrencyDao currencyDao,
        IWorkspaceAccessService workspaceAccessService,
        IPasswordService passwordService,
        IHttpCookiesService cookiesService,
        IUserDtoBuilder userDtoBuilder,
        ITimeEntryService timeEntryService,
        ITimeEntryApprovalService timeEntryApprovalService
    )
    {
        _authorizationService = authorizationService;
        _userDao = userDao;
        _workspaceDao = workspaceDao;
        _clientDao = clientDao;
        _projectDao = projectDao;
        _taskListDao = taskListDao;
        _taskDao = taskDao;
        _sessionProvider = sessionProvider;
        _paymentDao = paymentDao;
        _currencyDao = currencyDao;
        _workspaceAccessService = workspaceAccessService;
        _passwordService = passwordService;
        _cookiesService = cookiesService;
        _userDtoBuilder = userDtoBuilder;
        _timeEntryService = timeEntryService;
        _timeEntryApprovalService = timeEntryApprovalService;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginAsDemoRequest request)
    {
        var demoUser = await GetOrCreateDemoUserAsync();
        var targetMode = request.Mode ?? WorkspaceMode.Solo;

        var workspaces = await _userDao.GetUsersWorkspaces(demoUser);
        var targetWorkspace = workspaces.FirstOrDefault(w => w.Mode == targetMode)
            ?? workspaces.FirstOrDefault(w => w.IsDefault)
            ?? workspaces.FirstOrDefault();

        if (targetWorkspace != null)
        {
            await _userDao.SelectWorkspaceAsync(demoUser, targetWorkspace);
        }

        var loginResponse = await _authorizationService.Login(demoUser);
        var userDto = await _userDtoBuilder.BuildAsync(loginResponse.User);
        _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);

        return new LoginResponseDto
        {
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
        user.UserName = "john_deere";
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

        // Workspace 1 – main workspace (default), UTC / USD, Solo mode
        var ws1 = await _workspaceDao.CreateWorkspaceAsync(user, "🚀 My Startup", isDefault: true);
        await _workspaceAccessService.ShareAccessAsync(ws1, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws1, ws1.Name, usd, "UTC", "Main demo workspace");
        await _workspaceDao.SetModeAsync(ws1, WorkspaceMode.Solo);
        await SeedWorkspace1Async(ws1, user);

        // Workspace 2 – US timezone / EUR, Team mode
        var ws2 = await _workspaceDao.CreateWorkspaceAsync(user, "🌍 European Office");
        await _workspaceAccessService.ShareAccessAsync(ws2, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws2, ws2.Name, eur, "America/New_York", null);
        await _workspaceDao.SetModeAsync(ws2, WorkspaceMode.Team);
        await SeedWorkspace2Async(ws2, user);

        // Workspace 3 – Berlin / GBP (intentionally sparse), Solo mode
        var ws3 = await _workspaceDao.CreateWorkspaceAsync(user, "🏢 Berlin Hub");
        await _workspaceAccessService.ShareAccessAsync(ws3, user, MembershipAccessType.Owner);
        await _workspaceDao.UpdateWorkspaceAsync(ws3, ws3.Name, gbp, "Europe/Berlin", null);
        await _workspaceDao.SetModeAsync(ws3, WorkspaceMode.Solo);
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
        var project1 = await CreateDemoProjectAsync("🐛 Bug Fixes", 95, acmeClient);
        var project2 = await CreateDemoProjectAsync("🎨 Design System", 110, northstarClient);
        var project3 = await CreateDemoProjectAsync("⚙️ Platform Maintenance", 85, acmeClient);
        var project4 = await CreateDemoProjectAsync("📱 Mobile App", 105, brightAppsClient);

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

        await SeedMemberPaymentsAsync(ws, user, new[]
        {
            new DemoMemberPaymentSeed(project1, 620, -21, "Bug fixing retainer"),
            new DemoMemberPaymentSeed(project3, 380, -10, "Maintenance milestone"),
            new DemoMemberPaymentSeed(project2, 540, -14, "Design system phase 1"),
            new DemoMemberPaymentSeed(project4, 450, -5, "Mobile release deposit"),
            new DemoMemberPaymentSeed(project1, 250, -3, "General account credit")
        });
        await SeedTimeEntriesAsync(ws, user, tasks, approver: user, isTeamWorkspace: false);
    }

    private async Task SeedWorkspace2Async(WorkspaceEntity ws, UserEntity user)
    {
        var tasks = new List<TaskEntity>();

        var mediaClient = await _clientDao.CreateAsync(ws, "🌐 Global Media Group");
        var insightClient = await _clientDao.CreateAsync(ws, "📈 Insight Partners");
        var topClient = await _clientDao.CreateAsync(ws, "🏆 Top Client Inc");
        var qualityClient = await _clientDao.CreateAsync(ws, "🧪 Quality Works");

        var project1 = await CreateDemoProjectAsync("🚀 Launch Campaign", 120, mediaClient);
        var list1 = await _taskListDao.CreateTaskListAsync(project1, "Planning");
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Define campaign goals", status: TaskStatus.Done, priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Identify target audience", status: TaskStatus.Done));
        tasks.Add(await _taskDao.AddTaskAsync(list1, user, "Set KPIs", status: TaskStatus.InProgress));

        var list2 = await _taskListDao.CreateTaskListAsync(project1, "Execution");
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Create landing page copy"));
        tasks.Add(await _taskDao.AddTaskAsync(list2, user, "Design social media assets", priority: TaskPriority.Medium));

        var project2 = await CreateDemoProjectAsync("📊 Analytics Dashboard", 115, insightClient);
        var list3 = await _taskListDao.CreateTaskListAsync(project2, "Features");
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Implement funnel report"));
        tasks.Add(await _taskDao.AddTaskAsync(list3, user, "Add export to CSV"));

        await CreateDemoProjectAsync("Future Ideas", null, insightClient);

        var project3 = await CreateDemoProjectAsync("🔒 Security Audit", 140, topClient);
        var list4 = await _taskListDao.CreateTaskListAsync(project3, "Audit Checklist");
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Review authentication flows", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Check OWASP top 10", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list4, user, "Generate penetration test report"));

        var project4 = await CreateDemoProjectAsync("🧪 QA Automation", 100, qualityClient);
        var list5 = await _taskListDao.CreateTaskListAsync(project4, "Regression");
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Stabilize smoke tests", priority: TaskPriority.High));
        tasks.Add(await _taskDao.AddTaskAsync(list5, user, "Add reporting screenshots", priority: TaskPriority.Medium));

        // Add 2 team members for the Team workspace
        var member1 = await _userDao.CreatePendingUser($"sarah_{Guid.NewGuid():N}@team.timevic.com");
        member1.UserName = "sarah_connor";
        member1.VerificationTime = DateTime.UtcNow;
        _passwordService.SetUserPassword(member1, Guid.NewGuid().ToString());
        var member1Projects = new List<ProjectAccessModel>
        {
            new() { Project = project1, HourlyRate = 65 },
            new() { Project = project2, HourlyRate = 70 },
            new() { Project = project3, HourlyRate = 65 },
            new() { Project = project4, HourlyRate = 65 }
        };
        await _workspaceAccessService.ShareAccessAsync(ws, member1, MembershipAccessType.User, member1Projects);

        var member2 = await _userDao.CreatePendingUser($"alex_{Guid.NewGuid():N}@team.timevic.com");
        member2.UserName = "alex_murphy";
        member2.VerificationTime = DateTime.UtcNow;
        _passwordService.SetUserPassword(member2, Guid.NewGuid().ToString());
        var member2Projects = new List<ProjectAccessModel>
        {
            new() { Project = project1, HourlyRate = 60 },
            new() { Project = project2, HourlyRate = 60 },
            new() { Project = project3, HourlyRate = 75 },
            new() { Project = project4, HourlyRate = 75 }
        };
        await _workspaceAccessService.ShareAccessAsync(ws, member2, MembershipAccessType.User, member2Projects);

        await SeedMemberPaymentsAsync(ws, user, new[]
        {
            new DemoMemberPaymentSeed(project1, 780, -24, "Campaign kickoff payment"),
            new DemoMemberPaymentSeed(project1, 430, -7, "Launch copy approval"),
            new DemoMemberPaymentSeed(project2, 690, -17, "Analytics dashboard milestone"),
            new DemoMemberPaymentSeed(project3, 900, -11, "Security audit advance"),
            new DemoMemberPaymentSeed(project4, 360, -4, "QA automation setup"),
            new DemoMemberPaymentSeed(project3, 300, -2, "Client balance adjustment")
        });
        await SeedMemberPaymentsAsync(ws, member1, new[]
        {
            new DemoMemberPaymentSeed(project1, 520, -12, "Frontend implementation milestone"),
            new DemoMemberPaymentSeed(project2, 340, -5, "Analytics integration fee")
        });

        await SeedTimeEntriesAsync(ws, user, tasks, approver: user, isTeamWorkspace: true);
        await SeedTimeEntriesAsync(ws, member1, tasks.Take(5).ToList(), approver: user, isTeamWorkspace: true);
        await SeedTimeEntriesAsync(ws, member2, tasks.Skip(3).Take(5).ToList(), approver: user, isTeamWorkspace: true);
    }

    private async Task<ProjectEntity> CreateDemoProjectAsync(
        string name,
        decimal? defaultHourlyRate,
        ClientEntity client
    )
    {
        var project = await _projectDao.CreateAsync(client, name);
        project.DefaultHourlyRate = defaultHourlyRate;
        project.IsBillableByDefault = defaultHourlyRate.HasValue;
        return project;
    }

    private async Task SeedMemberPaymentsAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        IEnumerable<DemoMemberPaymentSeed> payments
    )
    {
        foreach (var payment in payments)
        {
            await _paymentDao.CreateAsync(
                workspace,
                user,
                payment.Project,
                payment.Amount,
                DateTime.UtcNow.Date.AddDays(payment.DayOffset).AddHours(12),
                payment.Description
            );
        }
    }

    private async Task SeedTimeEntriesAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        IReadOnlyCollection<TaskEntity> tasks,
        UserEntity approver,
        bool isTeamWorkspace = false
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

                var timeEntryDto = new TimeEntryCreationDto
                {
                    Description = task.Title,
                    IsBillable = project.IsBillableByDefault,
                    HourlyRate = project.DefaultHourlyRate,
                    StartTime = startTime,
                    EndTime = startTime.Add(duration)
                };

                var timeEntry = await _timeEntryService.SetAsync(user, workspace, timeEntryDto, project);
                timeEntry.Task = task;
                await _sessionProvider.CurrentSession.SaveAsync(timeEntry);

                if (isTeamWorkspace)
                {
                    var pattern = (index * 2 + entryIndex) % 5;
                    switch (pattern)
                    {
                        case 0 or 1:
                            await _timeEntryApprovalService.ApproveAsync(approver, timeEntry);
                            break;
                        case 2:
                            if (user.Id == approver.Id)
                            {
                                timeEntry.Status = TimeEntryStatus.Pending;
                                await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
                            }
                            else
                            {
                                await _timeEntryApprovalService.SubmitAsync(user, timeEntry);
                            }
                            break;
                        case 3:
                            timeEntry.Status = TimeEntryStatus.Draft;
                            await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
                            break;
                        default:
                            await _timeEntryApprovalService.RejectAsync(
                                approver,
                                timeEntry,
                                "Please provide additional details on this task."
                            );
                            break;
                    }
                }
                else
                {
                    if (timeEntry.Status == TimeEntryStatus.Approved && !timeEntry.Approvals.Any())
                    {
                        var approval = new TimeEntryApprovalEntity
                        {
                            TimeEntry = timeEntry,
                            User = approver,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        timeEntry.Approvals.Add(approval);
                        await _sessionProvider.CurrentSession.SaveAsync(approval);
                    }
                }
            }

            index++;
        }
    }

    private sealed record DemoMemberPaymentSeed(
        ProjectEntity Project,
        decimal Amount,
        int DayOffset,
        string Description
    );
    
}
