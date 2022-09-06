using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class UserSeeder: IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IRegistrationService _registrationService;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider,
        IRegistrationService registrationService,
        IDataFactory<UserEntity> userFactory,
        IJwtAuthService jwtAuthService
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _registrationService = registrationService;
        _userFactory = userFactory;
        _jwtAuthService = jwtAuthService;
    }

    public async Task<UserEntity> CreatePendingAsync()
    {
        var user = _userFactory.Generate();
        return await _registrationService.CreatePendingUser(user.Email);
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
}
