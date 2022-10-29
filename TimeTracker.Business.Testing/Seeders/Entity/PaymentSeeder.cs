using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class PaymentSeeder: IPaymentSeeder
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IClientSeeder _clientSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IDataFactory<PaymentEntity> _dataFactory;

    public PaymentSeeder(
        IDbSessionProvider sessionProvider,
        IPaymentDao paymentDao,
        IProjectSeeder projectSeeder,
        IClientSeeder clientSeeder,
        IUserSeeder userSeeder,
        IDataFactory<PaymentEntity> dataFactory
    )
    {
        _sessionProvider = sessionProvider;
        _paymentDao = paymentDao;
        _projectSeeder = projectSeeder;
        _clientSeeder = clientSeeder;
        _userSeeder = userSeeder;
        _dataFactory = dataFactory;
    }
    
    public async Task<ICollection<PaymentEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var workspace = user.Workspaces.First();
        var project = (await _projectSeeder.CreateSeveralAsync(workspace)).First();
        await _sessionProvider.PerformCommitAsync();
        return await CreateSeveralAsync(workspace, user, project.Client, project, count);
    }
    
    public async Task<ICollection<PaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, UserEntity user, int count = 1)
    {
        var project = (await _projectSeeder.CreateSeveralAsync(workspace)).First();
        await _sessionProvider.PerformCommitAsync();
        return await CreateSeveralAsync(workspace, user, project.Client, project, count);
    }

    public async Task<ICollection<PaymentEntity>> CreateSeveralAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ClientEntity client,
        ProjectEntity? project,
        int count = 1
    )
    {
        var result = new List<PaymentEntity>();
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
    
    public async Task<ICollection<PaymentEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        return await CreateSeveralAsync(user);
    }
}
