using System.Net;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Notes;

public class UpdateDraftTest : BaseTest
{
    private const string CreateDraftUrl = "/dashboard/notes/create-draft";
    private const string UpdateDraftUrl = "/dashboard/notes/update-draft";
    private const string CreateDocumentUrl = "/dashboard/notes/create-document";
    private const string CreateFolderUrl = "/dashboard/notes/create-folder";
    private const string GetContentUrl = "/dashboard/notes/get-content";

    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;

    public UpdateDraftTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task UpdateDraftRequiresAuthentication()
    {
        var response = await PostRequestAsAnonymousAsync(UpdateDraftUrl, new UpdateNoteDraftRequest
        {
            NoteId = Guid.NewGuid(),
            MarkdownContent = "test"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDraftRejectsEmptyNoteIdAndTooLongTitle()
    {
        var emptyIdResponse = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = Guid.Empty,
            MarkdownContent = "test"
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, emptyIdResponse.StatusCode);

        var draft = await CreateDraftAsync();
        var longTitleResponse = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = new string('x', 201)
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, longTitleResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateDraftReturnsBadRequestForMissingNote()
    {
        var response = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = Guid.NewGuid(),
            MarkdownContent = "updated"
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDraftRejectsRegularDocumentAndFolder()
    {
        var docResponse = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "Regular.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        docResponse.EnsureSuccessStatusCode();
        var doc = await docResponse.GetJsonDataAsync<NoteDocumentDto>();

        var folderResponse = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            Title = "Folder",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        folderResponse.EnsureSuccessStatusCode();
        var folder = await folderResponse.GetJsonDataAsync<NoteTreeNodeDto>();

        var updateDocAsDraft = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = doc.Id,
            MarkdownContent = "hack"
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, updateDocAsDraft.StatusCode);

        var updateFolderAsDraft = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = folder.Id,
            MarkdownContent = "hack"
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, updateFolderAsDraft.StatusCode);
    }

    [Fact]
    public async Task UpdateDraftRejectsOtherUserDraftEvenForWorkspaceOwner()
    {
        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var memberDraft = await CreateDraftAsync(memberToken);

        var response = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = memberDraft.Id,
            Title = "Owner hijack",
            MarkdownContent = "hijacked"
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDraftUpdatesTitleAndMarkdownContent()
    {
        // Tests interaction between update-draft and get-content
        var draft = await CreateDraftAsync();

        var updateResponse = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "Updated Draft Title",
            MarkdownContent = "{\"ok\":true}"
        }, _workspace.Id);
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.GetJsonDataAsync<NoteDocumentDto>();

        Assert.True(updated.IsDraft);
        Assert.Equal(NoteVisibility.Private, updated.Visibility);
        Assert.Equal("Updated Draft Title", updated.Title);
        Assert.NotNull(updated.LastContentId);
        Assert.NotEqual(draft.LastContentId, updated.LastContentId);

        var contentResponse = await PostRequestAsync(GetContentUrl, _jwtToken, new GetNoteContentRequest
        {
            ContentId = updated.LastContentId!.Value
        }, _workspace.Id);
        contentResponse.EnsureSuccessStatusCode();
        var content = await contentResponse.GetJsonDataAsync<NoteContentDto>();
        Assert.Equal("{\"ok\":true}", content.MarkdownContent);
    }

    [Fact]
    public async Task UpdateDraftRegeneratesTitleWhenWhitespaceProvided()
    {
        var draft = await CreateDraftAsync();

        var updateResponse = await PostRequestAsync(UpdateDraftUrl, _jwtToken, new UpdateNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "   ",
            MarkdownContent = "SELECT id, name FROM users"
        }, _workspace.Id);
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.GetJsonDataAsync<NoteDocumentDto>();

        Assert.Equal("SELECT id, name FROM", updated.Title);
    }

    private async Task<NoteDocumentDto> CreateDraftAsync(string? token = null)
    {
        var response = await PostRequestAsync(CreateDraftUrl, token ?? _jwtToken, new CreateNoteDraftRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteDocumentDto>();
    }
}
