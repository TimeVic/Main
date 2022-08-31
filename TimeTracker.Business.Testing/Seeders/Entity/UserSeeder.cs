using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class UserSeeder: IUserSeeder
{
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IDbSessionProvider _dbSessionProvider;

    public UserSeeder(
        IDbSessionProvider dbSessionProvider
    )
    {
        _dbSessionProvider = dbSessionProvider;
    }

    public async Task<UserEntity> CreateOneAsync()
    {
        var user = _userFactory.Generate();
        await _dbSessionProvider.CurrentSession.SaveAsync(user);
        return user;
    }
}
