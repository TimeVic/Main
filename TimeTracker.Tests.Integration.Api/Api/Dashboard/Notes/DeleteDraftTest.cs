using System.Net;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Notes;

public class DeleteDraftTest : BaseTest
{
    private const string CreateDraftUrl = "/dashboard/notes/create-draft";
    private const string DeleteDraftUrl = "/dashboard/notes/delete-draft";
    private const string CreateDocumentUrl = "/dashboard/notes/create-document";
    private const string CreateFolderUrl = "/dashboard/notes/create-folder";
    private const string GetTreeUrl = "/dashboard/notes/get-tree";
    private const string GetDocumentUrl = "/dashboard/notes/get-document";

    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;

    public DeleteDraftTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task DeleteDraftRequiresAuthentication()
    {
        var response = await PostRequestAsAnonymousAsync(DeleteDraftUrl, new DeleteNoteDraftRequest
        {
            NoteId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDraftRejectsEmptyNoteId()
    {
        var response = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = Guid.Empty
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDraftReturnsBadRequestForMissingNote()
    {
        var response = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = Guid.NewGuid()
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDraftRejectsRegularDocumentAndFolder()
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

        var deleteDocResponse = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = doc.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, deleteDocResponse.StatusCode);

        var deleteFolderResponse = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = folder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, deleteFolderResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteDraftRejectsOtherUserDraftEvenForWorkspaceOwner()
    {
        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var createResponse = await PostRequestAsync(CreateDraftUrl, memberToken, new CreateNoteDraftRequest(), _workspace.Id);
        createResponse.EnsureSuccessStatusCode();
        var memberDraft = await createResponse.GetJsonDataAsync<NoteDocumentDto>();

        var deleteResponse = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = memberDraft.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteDraftArchivesDraftAndExcludesFromTreeAndGetDocument()
    {
        // Tests interaction between delete-draft, get-tree, and get-document
        var createResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest
        {
            MarkdownContent = "temporary log line"
        }, _workspace.Id);
        createResponse.EnsureSuccessStatusCode();
        var draft = await createResponse.GetJsonDataAsync<NoteDocumentDto>();

        var deleteResponse = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = draft.Id
        }, _workspace.Id);
        deleteResponse.EnsureSuccessStatusCode();

        await FlushDbChanges(isClearSession: true);
        var archivedEntity = await DbSessionProvider.CurrentSession.GetAsync<NoteNodeEntity>(draft.Id);
        Assert.NotNull(archivedEntity);
        Assert.NotNull(archivedEntity.ArchivedAt);

        var treeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest
        {
            IncludeArchived = false,
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        treeResponse.EnsureSuccessStatusCode();
        var tree = await treeResponse.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.DoesNotContain(tree.Drafts, item => item.Id == draft.Id);
        Assert.DoesNotContain(tree.Nodes, item => item.Id == draft.Id);

        var includeArchivedTreeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest
        {
            IncludeArchived = true,
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        includeArchivedTreeResponse.EnsureSuccessStatusCode();
        var includeArchivedTree = await includeArchivedTreeResponse.GetJsonDataAsync<GetNotesTreeResponse>();
        Assert.DoesNotContain(includeArchivedTree.Drafts, item => item.Id == draft.Id);

        var getDocResponse = await PostRequestAsync(GetDocumentUrl, _jwtToken, new GetNoteDocumentRequest
        {
            NoteId = draft.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, getDocResponse.StatusCode);
    }
}
