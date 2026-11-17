using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Notes;

public class CreateDraftTest : BaseTest
{
    private const string CreateDraftUrl = "/dashboard/notes/create-draft";
    private const string DeleteDraftUrl = "/dashboard/notes/delete-draft";
    private const string GetTreeUrl = "/dashboard/notes/get-tree";
    private const string GetContentUrl = "/dashboard/notes/get-content";

    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;

    public CreateDraftTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task CreateDraftRequiresAuthentication()
    {
        var response = await PostRequestAsAnonymousAsync(CreateDraftUrl, new CreateNoteDraftRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftRequiresWorkspaceHeader()
    {
        var response = await PostWithoutWorkspaceHeaderAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftRejectsInvalidWorkspaceHeader()
    {
        var response = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftRejectsTooLongTitle()
    {
        var response = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest
        {
            Title = new string('a', 201)
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftGeneratesSequentialTitlesAndAlwaysPrivateInTeamWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Team;
        await FlushDbChanges();

        var firstResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest(), _workspace.Id);
        firstResponse.EnsureSuccessStatusCode();
        var first = await firstResponse.GetJsonDataAsync<NoteDocumentDto>();

        var secondResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest(), _workspace.Id);
        secondResponse.EnsureSuccessStatusCode();
        var second = await secondResponse.GetJsonDataAsync<NoteDocumentDto>();

        Assert.True(first.IsDraft);
        Assert.Equal(NoteVisibility.Private, first.Visibility);
        Assert.Null(first.ParentId);
        Assert.Equal("Draft 1", first.Title);
        Assert.NotNull(first.LastContentId);

        Assert.True(second.IsDraft);
        Assert.Equal(NoteVisibility.Private, second.Visibility);
        Assert.Equal("Draft 2", second.Title);

        // Sequence counter continues monotonically even after deleting a draft
        var deleteResponse = await PostRequestAsync(DeleteDraftUrl, _jwtToken, new DeleteNoteDraftRequest
        {
            NoteId = second.Id
        }, _workspace.Id);
        deleteResponse.EnsureSuccessStatusCode();

        var thirdResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest(), _workspace.Id);
        thirdResponse.EnsureSuccessStatusCode();
        var third = await thirdResponse.GetJsonDataAsync<NoteDocumentDto>();
        Assert.Equal("Draft 3", third.Title);
    }

    [Fact]
    public async Task CreateDraftDerivesTitleFromMarkdownFirstLineWhenTitleOmitted()
    {
        // Tests interaction between create-draft and get-content
        var response = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest
        {
            MarkdownContent = "# Investigate auth latency spikes today\nDetails here"
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var draft = await response.GetJsonDataAsync<NoteDocumentDto>();

        Assert.True(draft.IsDraft);
        Assert.Equal("Investigate auth latency spikes", draft.Title);
        Assert.NotNull(draft.LastContentId);

        var contentResponse = await PostRequestAsync(GetContentUrl, _jwtToken, new GetNoteContentRequest
        {
            ContentId = draft.LastContentId!.Value
        }, _workspace.Id);
        contentResponse.EnsureSuccessStatusCode();
        var content = await contentResponse.GetJsonDataAsync<NoteContentDto>();
        Assert.Equal("# Investigate auth latency spikes today\nDetails here", content.MarkdownContent);
    }

    [Fact]
    public async Task CreateDraftWorksInSoloWorkspaceAndForTeamUserRole()
    {
        var (memberToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var memberResponse = await PostRequestAsync(CreateDraftUrl, memberToken, new CreateNoteDraftRequest
        {
            Title = "Member scratchpad"
        }, _workspace.Id);
        memberResponse.EnsureSuccessStatusCode();
        var memberDraft = await memberResponse.GetJsonDataAsync<NoteDocumentDto>();
        Assert.True(memberDraft.IsDraft);
        Assert.Equal(NoteVisibility.Private, memberDraft.Visibility);
        Assert.Equal("Member scratchpad", memberDraft.Title);

        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var soloResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest(), _workspace.Id);
        soloResponse.EnsureSuccessStatusCode();
        var soloDraft = await soloResponse.GetJsonDataAsync<NoteDocumentDto>();
        Assert.True(soloDraft.IsDraft);
        Assert.Equal(NoteVisibility.Private, soloDraft.Visibility);
    }

    [Fact]
    public async Task CreateDraftAppearsInGetTreeDraftsOnlyInPrivateVisibilityAndIsolatedPerUser()
    {
        // Tests interaction between create-draft and get-tree
        _workspace.Mode = WorkspaceMode.Team;
        await FlushDbChanges();

        var (otherToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);

        var ownerDraftResponse = await PostRequestAsync(CreateDraftUrl, _jwtToken, new CreateNoteDraftRequest
        {
            Title = "Owner Draft"
        }, _workspace.Id);
        ownerDraftResponse.EnsureSuccessStatusCode();
        var ownerDraft = await ownerDraftResponse.GetJsonDataAsync<NoteDocumentDto>();

        var otherDraftResponse = await PostRequestAsync(CreateDraftUrl, otherToken, new CreateNoteDraftRequest
        {
            Title = "Other Draft"
        }, _workspace.Id);
        otherDraftResponse.EnsureSuccessStatusCode();
        var otherDraft = await otherDraftResponse.GetJsonDataAsync<NoteDocumentDto>();

        var ownerPrivateTreeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest
        {
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        ownerPrivateTreeResponse.EnsureSuccessStatusCode();
        var ownerPrivateTree = await ownerPrivateTreeResponse.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.Contains(ownerPrivateTree.Drafts, item => item.Id == ownerDraft.Id && item.IsDraft);
        Assert.DoesNotContain(ownerPrivateTree.Drafts, item => item.Id == otherDraft.Id);
        Assert.DoesNotContain(ownerPrivateTree.Nodes, item => item.Id == ownerDraft.Id);

        var ownerWorkspaceTreeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest
        {
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        ownerWorkspaceTreeResponse.EnsureSuccessStatusCode();
        var ownerWorkspaceTree = await ownerWorkspaceTreeResponse.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.Empty(ownerWorkspaceTree.Drafts);
        Assert.DoesNotContain(ownerWorkspaceTree.Nodes, item => item.Id == ownerDraft.Id);
    }

    private async Task<HttpResponseMessage> PostWithoutWorkspaceHeaderAsync(string url, string jwtToken, object data)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        return await HttpClient.PostAsync(url, JsonContent.Create(data));
    }
}
