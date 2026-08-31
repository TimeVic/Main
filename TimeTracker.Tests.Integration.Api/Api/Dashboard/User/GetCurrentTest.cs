using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class GetCurrentTest : BaseTest
{
    private const string Url = "/dashboard/user/current";
    private const string StorageUploadUrl = "/dashboard/storage/upload";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetCurrentTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await GetRequestAsAnonymousAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnCurrentUser()
    {
        var response = await GetRequestAsync(Url, _jwtToken);
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        Assert.Equal(_user.Id, actualUser.Id);
        Assert.Equal(_user.Email, actualUser.Email);
        Assert.Equal(_user.Login, actualUser.Login);
        Assert.Equal(_user.UserName, actualUser.UserName);
        Assert.Equal(_user.Timezone, actualUser.Timezone);
        Assert.NotNull(actualUser.DefaultWorkspace);
        Assert.Equal(_workspace.Id, actualUser.DefaultWorkspace.Id);
        Assert.NotNull(actualUser.SelectedWorkspace);
        Assert.Equal(_workspace.Id, actualUser.SelectedWorkspace.Id);
        Assert.NotNull(actualUser.Language);
        Assert.Equal("en", actualUser.Language.Code);
        Assert.Null(actualUser.Avatar);
    }

    [Fact]
    public async Task ShouldReturnSelectedWorkspace()
    {
        var selectedWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).First();
        await _userDao.SelectWorkspaceAsync(_user, selectedWorkspace);

        var response = await GetRequestAsync(Url, _jwtToken);
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        Assert.NotNull(actualUser.SelectedWorkspace);
        Assert.Equal(selectedWorkspace.Id, actualUser.SelectedWorkspace.Id);
    }

    [Fact]
    public async Task ShouldReturnDefaultWorkspaceIfSelectedWorkspaceAccessWasRemoved()
    {
        var selectedWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).First();
        await _userDao.SelectWorkspaceAsync(_user, selectedWorkspace);
        await FlushDbChanges();

        var member = selectedWorkspace.Members.First(item => item.User.Id == _user.Id);
        await _workspaceAccessService.RemoveAccessAsync(member.Id);

        var response = await GetRequestAsync(Url, _jwtToken);
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        Assert.NotNull(actualUser.SelectedWorkspace);
        Assert.Equal(_workspace.Id, actualUser.SelectedWorkspace.Id);
    }

    [Fact]
    public async Task ShouldReturnUploadedAvatar()
    {
        var uploadedAvatar = await UploadAvatarAsync();

        var response = await GetRequestAsync(Url, _jwtToken);
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        Assert.NotNull(actualUser.Avatar);
        Assert.Equal(uploadedAvatar.Id, actualUser.Avatar.Id);
        Assert.Equal(StoredFileType.Avatar, actualUser.Avatar.Type);
        Assert.Equal("image/jpeg", actualUser.Avatar.MimeType);
        Assert.Equal(uploadedAvatar.GetImageUrl(StorageImageSize.S_256), actualUser.Avatar.GetImageUrl(StorageImageSize.S_256));
    }

    private async Task<StoredFileDto> UploadAvatarAsync()
    {
        var response = await PostMultipartFormDataRequestAsync(
            StorageUploadUrl,
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

        return await response.GetJsonDataAsync<StoredFileDto>();
    }
}
