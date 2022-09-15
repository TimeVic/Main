using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Payment;

public class UpdateTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<ClientEntity> _clientFactory;
    private readonly IClientDao _clientDao;
    private readonly IPaymentDao _paymentDao;
    private readonly IDataFactory<PaymentEntity> _paymentFactory;

    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;

    public UpdateTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clientDao = Scope.Resolve<IClientDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
        _clientFactory = Scope.Resolve<IDataFactory<ClientEntity>>();
        _paymentFactory = Scope.Resolve<IDataFactory<PaymentEntity>>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        var payment = _paymentFactory.Generate();
        var client = await _clientDao.CreateAsync(_workspace, "Test");
        var project = await _projectDao.CreateAsync(_workspace, "Test");
        project.SetClient(client);
        
        var expectedPayment = _paymentFactory.Generate();
        var expectClient = await _clientDao.CreateAsync(_workspace, "Test2");
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test");
        expectProject.SetClient(expectClient);
        await DbSessionProvider.PerformCommitAsync();

        var actualPayment = await _paymentDao.CreateAsync(
            client,
            payment.Amount,
            payment.PaymentTime,
            project.Id,
            payment.Description
        );
        
        actualPayment = await _paymentDao.UpdatePaymentAsync(
            actualPayment.Id,
            expectClient,
            expectedPayment.Amount,
            expectedPayment.PaymentTime,
            expectProject.Id,
            expectedPayment.Description
        );
        
        Assert.True(actualPayment.Id > 0);
        Assert.Equal(expectedPayment.Amount, actualPayment.Amount);
        Assert.Equal(expectedPayment.Description, actualPayment.Description);
        Assert.Equal(expectedPayment.PaymentTime, actualPayment.PaymentTime);
        Assert.Equal(expectProject.Id, actualPayment.Project.Id);
        Assert.Equal(expectClient.Id, actualPayment.Client.Id);
    }
    
    [Fact]
    public async Task ShouldNotAddProjectNotForCurrentClient()
    {
        var payment = _paymentFactory.Generate();
        var client = await _clientDao.CreateAsync(_workspace, "Test");
        var project = await _projectDao.CreateAsync(_workspace, "Test");
        project.SetClient(client);
        
        var expectedPayment = _paymentFactory.Generate();
        var expectClient = await _clientDao.CreateAsync(_workspace, "Test2");
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test");
        await DbSessionProvider.PerformCommitAsync();

        var actualPayment = await _paymentDao.CreateAsync(
            client,
            payment.Amount,
            payment.PaymentTime,
            project.Id,
            payment.Description
        );
        
        actualPayment = await _paymentDao.UpdatePaymentAsync(
            actualPayment.Id,
            expectClient,
            expectedPayment.Amount,
            expectedPayment.PaymentTime,
            expectProject.Id,
            expectedPayment.Description
        );
        
        Assert.True(actualPayment.Id > 0);
        Assert.Null(actualPayment.Project);
    }
}
