using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Payment;

public class HasAccessTest: BaseTest
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
    private readonly IPaymentSeeder _paymentSeeder;

    public HasAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clientDao = Scope.Resolve<IClientDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
        _paymentSeeder = Scope.Resolve<IPaymentSeeder>();
        _clientFactory = Scope.Resolve<IDataFactory<ClientEntity>>();
        _paymentFactory = Scope.Resolve<IDataFactory<PaymentEntity>>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task ShouldHaveAccess()
    {
        var payment = (await _paymentSeeder.CreateSeveralAsync(_user)).First();

        var hasAccess = await _paymentDao.HasAccessAsync(_user, payment);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHaveNoAccess()
    {
        var payment = (await _paymentSeeder.CreateSeveralAsync()).First();

        var hasAccess = await _paymentDao.HasAccessAsync(_user, payment);
        Assert.True(hasAccess);
    }
}
