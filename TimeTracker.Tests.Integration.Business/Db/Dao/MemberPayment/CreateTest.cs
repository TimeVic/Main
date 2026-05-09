using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.MemberPayment;

public class CreateTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<ClientEntity> _clientFactory;
    private readonly IClientDao _clientDao;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IDataFactory<MemberPaymentEntity> _paymentFactory;

    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private IUserDao _userDao;

    public CreateTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clientDao = Scope.Resolve<IClientDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _paymentDao = Scope.Resolve<IMemberPaymentDao>();
        _clientFactory = Scope.Resolve<IDataFactory<ClientEntity>>();
        _paymentFactory = Scope.Resolve<IDataFactory<MemberPaymentEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldCreate()
    {
        var expectMemberPayment = _paymentFactory.Generate(); 
        var expectClient = await _clientDao.CreateAsync(_workspace, "Test");
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test");
        expectProject.SetClient(expectClient);
        await FlushDbChanges();

        var actualMemberPayment = await _paymentDao.CreateAsync(
            _workspace,
            _user,
            expectProject,
            expectMemberPayment.Amount,
            expectMemberPayment.PaymentTime,
            expectMemberPayment.Description
        );

        Assert.NotNull(actualMemberPayment);
        Assert.NotNull(actualMemberPayment.Project);
        Assert.NotEqual(Guid.Empty, actualMemberPayment.Id);
        Assert.Equal(expectMemberPayment.Amount, actualMemberPayment.Amount);
        Assert.Equal(expectMemberPayment.Description, actualMemberPayment.Description);
        Assert.Equal(expectMemberPayment.PaymentTime, actualMemberPayment.PaymentTime);
        Assert.Equal(expectProject.Id, actualMemberPayment.Project.Id);
        Assert.Equal(expectClient.Id, actualMemberPayment.Project.Client!.Id);
    }
    
    [Fact]
    public async Task ShouldCreateForProjectWithoutClient()
    {
        var expectMemberPayment = _paymentFactory.Generate(); 
        var expectProject = await _projectDao.CreateAsync(_workspace, "Test");
        await FlushDbChanges();
       
        var actualMemberPayment = await _paymentDao.CreateAsync(
            _workspace,
            _user,
            expectProject,
            expectMemberPayment.Amount,
            expectMemberPayment.PaymentTime,
            expectMemberPayment.Description
        );
        
        Assert.Equal(expectProject.Id, actualMemberPayment.Project.Id);
        Assert.Null(actualMemberPayment.Project.Client);
    }
}
