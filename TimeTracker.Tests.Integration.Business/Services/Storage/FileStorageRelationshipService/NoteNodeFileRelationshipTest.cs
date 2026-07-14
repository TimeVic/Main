using Autofac;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.FileStorageRelationshipService;

public class NoteNodeFileRelationshipTest : BaseTest
{
    private readonly IFileStorageRelationshipService _relationshipService;
    private readonly INoteDao _noteDao;
    private readonly IUserDao _userDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly NoteNodeEntity _note;

    public NoteNodeFileRelationshipTest() : base()
    {
        _relationshipService = Scope.Resolve<IFileStorageRelationshipService>();
        _noteDao = Scope.Resolve<INoteDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = Scope.Resolve<IUserSeeder>().CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        _note = _noteDao.CreateNodeAsync(
            _workspace,
            null,
            _user,
            NoteNodeType.Document,
            "Attachment note",
            string.Empty,
            NoteVisibility.Private,
            1000
        ).Result;
    }

    [Fact]
    public async Task ShouldGetNoteNodeRelationship()
    {
        await FlushDbChanges();

        var relationship = await _relationshipService.GetFileRelationship(_note.Id, StorageEntityType.NoteNode);

        var actualNote = Assert.IsType<NoteNodeEntity>(relationship);
        Assert.Equal(_note.Id, actualNote.Id);
    }

    [Fact]
    public async Task ShouldAddFileRelationshipToNoteNode()
    {
        var file = new StoredFileEntity
        {
            Type = StoredFileType.Attachment,
            CloudFilePath = "note/test-file.pdf",
            MimeType = "application/pdf",
            OriginalFileName = "test-file.pdf",
            Size = 1,
            CreatedAt = DateTime.UtcNow
        };
        await DbSessionProvider.CurrentSession.SaveAsync(file);

        await _relationshipService.AddFileRelationship(_note, file);
        await FlushDbChanges(isClearSession: true);

        var actualNote = await DbSessionProvider.CurrentSession.GetAsync<NoteNodeEntity>(_note.Id);
        var actualFile = await DbSessionProvider.CurrentSession.Query<StoredFileEntity>()
            .FetchMany(item => item.NoteNodes)
            .FirstAsync(item => item.Id == file.Id);

        Assert.Contains(actualNote.Attachments, item => item.Id == file.Id);
        Assert.Contains(actualFile.NoteNodes, item => item.Id == _note.Id);
    }
}
