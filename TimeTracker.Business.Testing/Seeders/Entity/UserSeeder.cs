using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class UserSeeder: IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IRegistrationService _registrationService;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider,
        IRegistrationService registrationService,
        IDataFactory<UserEntity> userFactory,
        IJwtAuthService jwtAuthService,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _registrationService = registrationService;
        _userFactory = userFactory;
        _jwtAuthService = jwtAuthService;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<UserEntity> CreatePendingAsync()
    {
        var user = _userFactory.Generate();
        return await _registrationService.CreatePendingUser(user.Email);
    }

    public async Task<ICollection<UserEntity>> CreateActivatedAsync(int counter, string password = "test password")
    {
        var users = new List<UserEntity>();
        for (int i = 0; i < counter; i++)
        {
            users.Add(await CreateActivatedAsync(password));
        }

        return users;
    }
    
    public async Task<UserEntity> CreateActivatedAsync(string password = "test password")
    {
        var user = await CreatePendingAsync();
        user = await _registrationService.ActivateUser(user.VerificationToken, password);
        await _dbSessionProvider.PerformCommitAsync();
        return user;
    }
    
    public async Task<(string token, UserEntity user)> CreateAuthorizedAsync(string password = "test password")
    {
        var user = await CreateActivatedAsync(password);
        return (
            _jwtAuthService.BuildJwt(user.Id),
            user
        );
    }
    
    public async Task<(string token, UserEntity user)> CreateAuthorizedAndShareAsync(
        WorkspaceEntity workspace,
        MembershipAccessType access = MembershipAccessType.User,
        ICollection<ProjectAccessModel>? projects = null
    )
    {
        var user = await CreateActivatedAndShareAsync(workspace, access, projects);
        return (
            _jwtAuthService.BuildJwt(user.Id),
            user
        );
    }
    
    public async Task<UserEntity> CreateActivatedAndShareAsync(
        WorkspaceEntity workspace,
        MembershipAccessType access = MembershipAccessType.User,
        ICollection<ProjectAccessModel>? projects = null
    )
    {
        var user = await CreatePendingAsync();
        user = await _registrationService.ActivateUser(user.VerificationToken, "Test password");
        await _dbSessionProvider.PerformCommitAsync();
        await _workspaceAccessService.ShareAccessAsync(workspace, user, access, projects);
        return user;
    }
}
