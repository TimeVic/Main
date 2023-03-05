using System.Drawing;
using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.StoredFile;

public class GetListByEntityTest: BaseTest
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
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IStoredFilesDao _storedFilesDao;

    public GetListByEntityTest(): base()
    {
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _tagFactory = Scope.Resolve<IDataFactory<TagEntity>>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _tagDao = Scope.Resolve<ITagDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _userDao = Scope.Resolve<IUserDao>();
        _fileStorage = Scope.Resolve<IFileStorage>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldGetListForTask()
    {
        var task = await _taskSeeder.CreateAsync();
        
        var actualFile1 = await _fileStorage.PutFileAsync(task, CreateFormFile(), StoredFileType.Attachment);
        var actualFile2 = await _fileStorage.PutFileAsync(task, CreateFormFile(), StoredFileType.Attachment);
        var actualFile3 = await _fileStorage.PutFileAsync(task, CreateFormFile("images/image.jpg"), StoredFileType.Image);
        await CommitDbChanges();

        var actualFiles = await _storedFilesDao.GetListByEntity(task.Id, StorageEntityType.Task);
        
        Assert.Equal(3, actualFiles.Count);
        Assert.Contains(actualFiles, items => items.Type == StoredFileType.Image);
        Assert.Contains(actualFiles, items => items.Type == StoredFileType.Attachment);
    }
}
