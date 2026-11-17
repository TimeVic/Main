using System.Net;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Notes;

public class PromoteDraftTest : BaseTest
{
    private const string CreateDraftUrl = "/dashboard/notes/create-draft";
    private const string PromoteDraftUrl = "/dashboard/notes/promote-draft";
    private const string CreateDocumentUrl = "/dashboard/notes/create-document";
    private const string CreateFolderUrl = "/dashboard/notes/create-folder";
    private const string GetTreeUrl = "/dashboard/notes/get-tree";
    private const string GetContentUrl = "/dashboard/notes/get-content";
    private const string GetHistoryUrl = "/dashboard/notes/get-history";

    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;

    public PromoteDraftTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task PromoteDraftRequiresAuthentication()
    {
        var response = await PostRequestAsAnonymousAsync(PromoteDraftUrl, new PromoteNoteDraftRequest
        {
            NoteId = Guid.NewGuid(),
            Title = "Promoted.md",
            Visibility = NoteVisibility.Private
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftRejectsEmptyNoteIdAndEmptyTitle()
    {
        var emptyIdResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = Guid.Empty,
            Title = "Promoted.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, emptyIdResponse.StatusCode);

        var draft = await CreateDraftAsync();
        var emptyTitleResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = string.Empty,
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, emptyTitleResponse.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftReturnsBadRequestForMissingNote()
    {
        var response = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = Guid.NewGuid(),
            Title = "Promoted.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftRejectsRegularDocumentAndFolder()
    {
        var docResponse = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "Regular.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        docResponse.EnsureSuccessStatusCode();
        var doc = await docResponse.GetJsonDataAsync<NoteDocumentDto>();

        var folder = await CreateFolderAsync("Folder", NoteVisibility.Private);

        var promoteDoc = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = doc.Id,
            Title = "StillRegular",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, promoteDoc.StatusCode);

        var promoteFolder = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = folder.Id,
            Title = "StillFolder",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, promoteFolder.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftRejectsOtherUserDraftEvenForWorkspaceOwner()
    {
        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var memberDraft = await CreateDraftAsync(memberToken);

        var response = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = memberDraft.Id,
            Title = "Hijack",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftRejectsWorkspaceVisibilityInSoloWorkspaceAndForUserRole()
    {
        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var memberDraft = await CreateDraftAsync(memberToken);

        var memberPromoteWorkspaceResponse = await PostRequestAsync(PromoteDraftUrl, memberToken, new PromoteNoteDraftRequest
        {
            NoteId = memberDraft.Id,
            Title = "Unauthorized Shared",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, memberPromoteWorkspaceResponse.StatusCode);

        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var ownerDraft = await CreateDraftAsync();
        var soloPromoteWorkspaceResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = ownerDraft.Id,
            Title = "Workspace In Solo",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, soloPromoteWorkspaceResponse.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftRejectsMismatchedParentVisibilityAndDocumentParent()
    {
        _workspace.Mode = WorkspaceMode.Team;
        await FlushDbChanges();

        var workspaceFolder = await CreateFolderAsync("Shared", NoteVisibility.Workspace);
        var privateFolder = await CreateFolderAsync("Private", NoteVisibility.Private);
        var docResponse = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "Doc.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        docResponse.EnsureSuccessStatusCode();
        var regularDoc = await docResponse.GetJsonDataAsync<NoteDocumentDto>();

        var draft = await CreateDraftAsync();

        var mismatchWorkspaceInPrivate = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "Promoted",
            Visibility = NoteVisibility.Workspace,
            ParentId = privateFolder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, mismatchWorkspaceInPrivate.StatusCode);

        var mismatchPrivateInWorkspace = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "Promoted",
            Visibility = NoteVisibility.Private,
            ParentId = workspaceFolder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, mismatchPrivateInWorkspace.StatusCode);

        var docParentResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "Promoted",
            Visibility = NoteVisibility.Private,
            ParentId = regularDoc.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, docParentResponse.StatusCode);
    }

    [Fact]
    public async Task PromoteDraftConvertsToPrivateDocumentInFolderAndRecordsHistory()
    {
        // Tests interaction between promote-draft, get-tree, get-content, and get-history
        var privateFolder = await CreateFolderAsync("Specs", NoteVisibility.Private);
        var draft = await CreateDraftAsync(markdownContent: "# Initial spec");

        var promoteResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "API Specification",
            Visibility = NoteVisibility.Private,
            ParentId = privateFolder.Id,
            MarkdownContent = "# Final API Specification"
        }, _workspace.Id);
        promoteResponse.EnsureSuccessStatusCode();
        var promoted = await promoteResponse.GetJsonDataAsync<NoteDocumentDto>();

        Assert.False(promoted.IsDraft);
        Assert.Equal(NoteVisibility.Private, promoted.Visibility);
        Assert.Equal(privateFolder.Id, promoted.ParentId);
        Assert.Equal("API Specification", promoted.Title);
        Assert.NotNull(promoted.LastContentId);

        var treeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest
        {
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        treeResponse.EnsureSuccessStatusCode();
        var tree = await treeResponse.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.DoesNotContain(tree.Drafts, item => item.Id == promoted.Id);
        Assert.Contains(tree.Nodes, item =>
            item.Id == promoted.Id
            && !item.IsDraft
            && item.ParentId == privateFolder.Id
            && item.Title == "API Specification"
        );

        var contentResponse = await PostRequestAsync(GetContentUrl, _jwtToken, new GetNoteContentRequest
        {
            ContentId = promoted.LastContentId!.Value
        }, _workspace.Id);
        contentResponse.EnsureSuccessStatusCode();
        var content = await contentResponse.GetJsonDataAsync<NoteContentDto>();
        Assert.Equal("# Final API Specification", content.MarkdownContent);

        var historyResponse = await PostRequestAsync(GetHistoryUrl, _jwtToken, new GetNoteNodeHistoryRequest
        {
            NoteId = promoted.Id
        }, _workspace.Id);
        historyResponse.EnsureSuccessStatusCode();
        var history = await historyResponse.GetJsonDataAsync<GetNoteNodeHistoryResponse>();
        Assert.NotEmpty(history.History);
        Assert.Contains(history.History, item => item.Title == "API Specification");
    }

    [Fact]
    public async Task PromoteDraftConvertsToWorkspaceDocumentVisibleToTeamMembers()
    {
        // Tests interaction between promote-draft and get-tree across workspace members
        _workspace.Mode = WorkspaceMode.Team;
        await FlushDbChanges();

        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var sharedFolder = await CreateFolderAsync("Team Docs", NoteVisibility.Workspace);
        var draft = await CreateDraftAsync(markdownContent: "Team onboarding checklist");

        var promoteResponse = await PostRequestAsync(PromoteDraftUrl, _jwtToken, new PromoteNoteDraftRequest
        {
            NoteId = draft.Id,
            Title = "Onboarding",
            Visibility = NoteVisibility.Workspace,
            ParentId = sharedFolder.Id
        }, _workspace.Id);
        promoteResponse.EnsureSuccessStatusCode();
        var promoted = await promoteResponse.GetJsonDataAsync<NoteDocumentDto>();

        Assert.False(promoted.IsDraft);
        Assert.Equal(NoteVisibility.Workspace, promoted.Visibility);
        Assert.Equal(sharedFolder.Id, promoted.ParentId);

        var memberTreeResponse = await PostRequestAsync(GetTreeUrl, memberToken, new GetNotesTreeRequest
        {
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        memberTreeResponse.EnsureSuccessStatusCode();
        var memberTree = await memberTreeResponse.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.Contains(memberTree.Nodes, item => item.Id == promoted.Id && item.ParentId == sharedFolder.Id);
        Assert.Empty(memberTree.Drafts);
    }

    private async Task<NoteDocumentDto> CreateDraftAsync(string? token = null, string? markdownContent = null)
    {
        var response = await PostRequestAsync(CreateDraftUrl, token ?? _jwtToken, new CreateNoteDraftRequest
        {
            MarkdownContent = markdownContent
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteDocumentDto>();
    }

    private async Task<NoteTreeNodeDto> CreateFolderAsync(string title, NoteVisibility visibility)
    {
        var response = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            Title = title,
            Visibility = visibility
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteTreeNodeDto>();
    }
}
