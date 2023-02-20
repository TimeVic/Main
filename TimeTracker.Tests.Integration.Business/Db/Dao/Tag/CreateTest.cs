using System.Drawing;
using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tag;

public class CreateTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly ITagDao _tagDao;

    public CreateTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _tagFactory = Scope.Resolve<IDataFactory<TagEntity>>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _tagDao = Scope.Resolve<ITagDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldCreate()
    {
        Color red = ColorTranslator.FromHtml("#FF0000");
        var expectedTag = _tagFactory.Generate();
        var actualTag = await _tagDao.CreateAsync(_workspace, expectedTag.Name, red);
        Assert.NotNull(actualTag);
        await CommitDbChanges();

        actualTag = await DbSessionProvider.CurrentSession.GetAsync<TagEntity>(actualTag.Id);
        Assert.Equal(expectedTag.Name, actualTag.Name);
        Assert.Equal(_workspace.Id, actualTag.Workspace.Id);
        Assert.Equal(red, actualTag.Color);
    }
    
    [Fact]
    public async Task ShouldCreateWithDefaultColor()
    {
        var expectedTag = _tagFactory.Generate();
        var actualTag = await _tagDao.CreateAsync(_workspace, expectedTag.Name, Color.Aquamarine);
        Assert.NotNull(actualTag);
        await CommitDbChanges();

        actualTag = await DbSessionProvider.CurrentSession.GetAsync<TagEntity>(actualTag.Id);
        Assert.Equal(expectedTag.Name, actualTag.Name);
        Assert.Equal(_workspace.Id, actualTag.Workspace.Id);
        Assert.Equal(Color.Aquamarine.ToHexString(), actualTag.Color?.ToHexString());
    }
}
