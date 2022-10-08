using Autofac;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Payment;

public class GetListTest: BaseTest
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

    public GetListTest(): base()
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
    public async Task ShouldReceiveList()
    {
        var expectedTotal = 30;
        await _paymentSeeder.CreateSeveralAsync(_user, expectedTotal);

        var listModel = await _paymentDao.GetListAsync(_user.DefaultWorkspace, 1);
        Assert.Equal(PaginationUtils.DefaultPageSize, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
        
        Assert.All(listModel.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotNull(item.Client);
            Assert.NotEmpty(item.Description);
            Assert.True(item.Amount > 0);
            Assert.True(item.PaymentTime > DateTime.MinValue);
        });
        
        // The ordering is correct
        Assert.True(
            listModel.Items.First().PaymentTime > listModel.Items.Skip(1).First().PaymentTime    
        );
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherWorkspaceReceiveList()
    {
        var expectedTotal = 7;
        await _paymentSeeder.CreateSeveralAsync(_user, expectedTotal);
        var otherWorkspace = await _workspaceDao.CreateWorkspaceAsync(_user, "Test 2");
        var otherClient = await _clientDao.CreateAsync(otherWorkspace, "Test");
        await _paymentSeeder.CreateSeveralAsync(otherWorkspace, _user, otherClient, null, 15);
        
        var listModel = await _paymentDao.GetListAsync(_user.DefaultWorkspace, 1);
        Assert.Equal(expectedTotal, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
    }
}
