using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class UserSeeder: IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IRegistrationService _registrationService;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider,
        IRegistrationService registrationService,
        IDataFactory<UserEntity> userFactory
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _registrationService = registrationService;
        _userFactory = userFactory;
    }

    public async Task<UserEntity> CreateActivatedAsync(string password = "test password")
    {
        var user = _userFactory.Generate();
        user = await _registrationService.CreatePendingUser(user.Email);
        user = await _registrationService.ActivateUser(user.VerificationToken, password);
        return user;
    }
}
