using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Storage;

public class UploadTest: BaseTest
{
    private readonly string Url = "/dashboard/storage/upload";

    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly string _jwtToken;
    private readonly IProjectDao _projectDao;
    private readonly INoteDao _noteDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;

    private readonly TaskEntity _task;
    private readonly IFileStorage _fileStorage;
    private WorkspaceEntity _workspace;

    public UploadTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _noteDao = ServiceProvider.GetRequiredService<INoteDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();

        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            data: new Dictionary<string, object>()
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            file: CreateFormFile()
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpload()
    {
        Assert.Equal(0, _task.Attachments.Count);

        var fileToUpload = CreateFormFile("image.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        Assert.NotEqual(Guid.Empty, actualData.Id);
        Assert.NotEmpty(actualData.Url);
        Assert.NotEmpty(actualData.GetImageUrl(TimeTracker.Business.Common.Constants.Storage.StorageImageSize.S_256));

        await FlushDbChanges(true);
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        Assert.Equal(1, actualTask.Attachments.Count);

        var actualUploadedFile = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(actualData.Id);
        Assert.NotNull(actualUploadedFile);
        Assert.NotEmpty(actualUploadedFile.ThumbCloudFilePath!);
    }

    [Fact]
    public async Task ShouldUploadAttachmentToNote()
    {
        var note = await _noteDao.CreateNodeAsync(
            _workspace,
            null,
            _user,
            NoteNodeType.Document,
            "Upload.md",
            string.Empty,
            NoteVisibility.Workspace,
            1000
        );
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", note.Id },
                { "EntityType", StorageEntityType.NoteNode },
                { "FileType", StoredFileType.Attachment }
            },
            CreateFormFile("image.jpg")
        );
        response.EnsureSuccessStatusCode();

        var uploadedFile = await response.GetJsonDataAsync<StoredFileDto>();
        await FlushDbChanges(true);
        var actualNote = await DbSessionProvider.CurrentSession.GetAsync<NoteNodeEntity>(note.Id);

        Assert.NotEqual(Guid.Empty, uploadedFile.Id);
        Assert.Single(actualNote.Attachments);
        Assert.Equal(uploadedFile.Id, actualNote.Attachments.Single().Id);
    }

    [Fact]
    public async Task ShouldUploadMarkdownAttachmentToNote()
    {
        var note = await _noteDao.CreateNodeAsync(
            _workspace,
            null,
            _user,
            NoteNodeType.Document,
            "Markdown.md",
            string.Empty,
            NoteVisibility.Workspace,
            1000
        );
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", note.Id },
                { "EntityType", StorageEntityType.NoteNode },
                { "FileType", StoredFileType.Attachment }
            },
            CreateFormFile("deployment.md")
        );
        response.EnsureSuccessStatusCode();

        var uploadedFile = await response.GetJsonDataAsync<StoredFileDto>();

        Assert.Equal("md", uploadedFile.Extension);
        Assert.Equal("text/markdown", uploadedFile.MimeType);
    }

    [Fact]
    public async Task ShouldUploadIfHasNotAccessToEntity()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var task = _taskSeeder.CreateAsync(user: user2).Result;

        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "WorkspaceId", otherWorkspace.Id },
                { "EntityId", task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            CreateFormFile("image.jpg")
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldUploadBigJpgFile()
    {
        Assert.Equal(0, _task.Attachments.Count);

        var fileToUpload = CreateFormFile("big.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        Assert.NotEqual(Guid.Empty, actualData.Id);
        Assert.NotEmpty(actualData.Url);
        Assert.NotEmpty(actualData.GetImageUrl(TimeTracker.Business.Common.Constants.Storage.StorageImageSize.S_256));

        await FlushDbChanges(true);
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        Assert.Equal(1, actualTask.Attachments.Count);

        var actualUploadedFile = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(actualData.Id);
        Assert.NotNull(actualUploadedFile);
        Assert.NotEmpty(actualUploadedFile.ThumbCloudFilePath!);
    }

    [Fact]
    public async Task ShouldUploadUserAvatar()
    {
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", _user.Id },
                { "EntityType", StorageEntityType.User },
                { "FileType", StoredFileType.Avatar },
            },
            CreateFormFile("image.jpg")
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        Assert.Equal(StoredFileType.Avatar, actualData.Type);
        Assert.Equal("image/jpeg", actualData.MimeType);

        await FlushDbChanges(true);
        var actualUploadedFile = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(actualData.Id);
        Assert.NotNull(actualUploadedFile);
        Assert.Equal("jpg", actualUploadedFile.Extension);
        Assert.NotEmpty(actualUploadedFile.ThumbCloudFilePath!);

        var actualUser = await DbSessionProvider.CurrentSession.GetAsync<UserEntity>(_user.Id);
        Assert.Single(actualUser.Avatars);

        var fileResponse = await GetRequestAsync(actualData.Url, _jwtToken);
        fileResponse.EnsureSuccessStatusCode();
        await using var imageStream = await fileResponse.Content.ReadAsStreamAsync();
        using var image = await Image.LoadAsync(imageStream);
        Assert.NotEqual(0, image.Width);
        Assert.NotEqual(0, image.Height);
    }

    [Theory]
    [InlineData(StorageImageSize.Xxs_64, 64)]
    [InlineData(StorageImageSize.S_256, 256)]
    [InlineData(StorageImageSize.M_400, 400)]
    public async Task ShouldUploadAndDownloadImageWithRequestedSize(StorageImageSize imageSize, int expectedSize)
    {
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "WorkspaceId", _workspace.Id },
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            CreateFormFile("image.jpg")
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        var fileResponse = await GetRequestAsync(
            actualData.Url,
            _jwtToken,
            new Dictionary<string, string>()
            {
                { "imageSize", imageSize.ToString() }
            }
        );
        fileResponse.EnsureSuccessStatusCode();
        Assert.Equal("image/png", fileResponse.Content.Headers.ContentType?.MediaType);

        await using var imageStream = await fileResponse.Content.ReadAsStreamAsync();
        using var image = await Image.LoadAsync(imageStream);
        Assert.Equal(expectedSize, image.Width);
        Assert.Equal(expectedSize, image.Height);
    }
}
