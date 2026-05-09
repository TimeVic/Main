using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.MemberPayment;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<ClientEntity> _clientFactory;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IDataFactory<MemberPaymentEntity> _paymentFactory;

    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly IUserDao _userDao;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _paymentDao = Scope.Resolve<IMemberPaymentDao>();
        _paymentSeeder = Scope.Resolve<IMemberPaymentSeeder>();
        _clientFactory = Scope.Resolve<IDataFactory<ClientEntity>>();
        _paymentFactory = Scope.Resolve<IDataFactory<MemberPaymentEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedTotal = 30;
        await _paymentSeeder.CreateSeveralAsync(_user, expectedTotal);

        await FlushDbChanges();
        var listModel = await _paymentDao.GetListAsync(_workspace, _user, 1);
        Assert.Equal(PaginationUtils.DefaultPageSize, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
        
        Assert.All(listModel.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.NotNull(item.Project);
            Assert.NotEmpty(item.Description!);
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
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var otherWorkspace = (await _userDao.GetUsersWorkspaces(otherUser, MembershipAccessType.Owner)).First();
        var otherProject = await _projectSeeder.CreateAsync(otherWorkspace);
        var otherClient = otherProject.Client;
        await _paymentSeeder.CreateSeveralAsync(otherWorkspace, otherUser, otherProject, 15);

        await FlushDbChanges();
        var listModel = await _paymentDao.GetListAsync(_workspace, _user, 1);
        Assert.Equal(expectedTotal, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUser()
    {
        var expectedTotal = 7;
        await _paymentSeeder.CreateSeveralAsync(_user, expectedTotal);
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var otherWorkspace = (await _userDao.GetUsersWorkspaces(otherUser, MembershipAccessType.Owner)).First();
        var otherProject = await _projectSeeder.CreateAsync(otherWorkspace);
        var otherClient = otherProject.Client;
        await _paymentSeeder.CreateSeveralAsync(otherWorkspace, otherUser, otherProject, 15);
        
        await FlushDbChanges();
        var listModel = await _paymentDao.GetListAsync(_workspace, _user, 1);
        Assert.Equal(expectedTotal, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUserWithoutMemberPaymentsForOtherUsers()
    {
        var expectedTotal = 7;
        await _paymentSeeder.CreateSeveralAsync(_user, 12);
        
        var otherUser = await _userSeeder.CreateActivatedAndShareAsync(_workspace);
        await FlushDbChanges();
        await _paymentSeeder.CreateSeveralAsync(_workspace, otherUser, expectedTotal);

        await FlushDbChanges();
        var listModel = await _paymentDao.GetListAsync(_workspace, otherUser, 1);
        Assert.Equal(expectedTotal, listModel.Items.Count);
        Assert.Equal(expectedTotal, listModel.TotalCount);
    }
}
