using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class ClientPaymentSeeder: IClientPaymentSeeder
{
    private readonly IClientPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IDataFactory<ClientPaymentEntity> _dataFactory;

    public ClientPaymentSeeder(
        IClientPaymentDao paymentDao,
        IProjectSeeder projectSeeder,
        IUserSeeder userSeeder,
        IDataFactory<ClientPaymentEntity> dataFactory
    )
    {
        _paymentDao = paymentDao;
        _projectSeeder = projectSeeder;
        _userSeeder = userSeeder;
        _dataFactory = dataFactory;
    }

    public async Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1)
    {
        var project = (await _projectSeeder.CreateSeveralAsync(workspace)).First();
        return await CreateSeveralAsync(workspace, project.Client!, project, count);
    }

    public async Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(
        WorkspaceEntity workspace,
        ClientEntity client,
        ProjectEntity? project,
        int count = 1
    )
    {
        var result = new List<ClientPaymentEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _dataFactory.Generate();
            var entry = await _paymentDao.CreateAsync(
                workspace,
                client,
                fakeEntry.Amount,
                fakeEntry.PaymentTime,
                project?.Id,
                fakeEntry.Description
            );
            result.Add(entry);
        }

        return result;
    }

    public async Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(int count = 1)
    {
        var (_, _, workspace) = await _userSeeder.CreateAuthorizedAsync();
        return await CreateSeveralAsync(workspace, count);
    }
}
