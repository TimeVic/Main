using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class MemberPaymentSeeder: IMemberPaymentSeeder
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IClientSeeder _clientSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IDataFactory<MemberPaymentEntity> _dataFactory;
    private readonly IUserDao _userDao;

    public MemberPaymentSeeder(
        IDbSessionProvider sessionProvider,
        IMemberPaymentDao paymentDao,
        IProjectSeeder projectSeeder,
        IClientSeeder clientSeeder,
        IUserSeeder userSeeder,
        IDataFactory<MemberPaymentEntity> dataFactory,
        IUserDao userDao
    )
    {
        _sessionProvider = sessionProvider;
        _paymentDao = paymentDao;
        _projectSeeder = projectSeeder;
        _clientSeeder = clientSeeder;
        _userSeeder = userSeeder;
        _dataFactory = dataFactory;
        _userDao = userDao;
    }
    
    public async Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var workspace = (await _userDao.GetUsersWorkspaces(user, MembershipAccessType.Owner)).First();
        var project = (await _projectSeeder.CreateSeveralAsync(workspace)).First();
        return await CreateSeveralAsync(workspace, user, project.Client!, project, count);
    }
    
    public async Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, UserEntity user, int count = 1)
    {
        var project = (await _projectSeeder.CreateSeveralAsync(workspace)).First();
        return await CreateSeveralAsync(workspace, user, project.Client!, project, count);
    }

    public async Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ClientEntity client,
        ProjectEntity? project,
        int count = 1
    )
    {
        var result = new List<MemberPaymentEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _dataFactory.Generate();
            var entry = await _paymentDao.CreateAsync(
                workspace,
                user,
                client, 
                fakeEntry.Amount,
                fakeEntry.PaymentTime,
                project?.Id,
                fakeEntry.Description
            );;
            result.Add(entry);
        }

        return result;
    }
    
    public async Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        return await CreateSeveralAsync(user);
    }
}
