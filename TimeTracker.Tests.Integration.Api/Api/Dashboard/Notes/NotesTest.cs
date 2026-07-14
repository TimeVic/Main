using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Controllers.Dashboard.Notes;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Notes;

public class NotesTest : BaseTest
{
    private const string GetTreeUrl = "/dashboard/notes/get-tree";
    private const string GetDocumentUrl = "/dashboard/notes/get-document";
    private const string GetContentUrl = "/dashboard/notes/get-content";
    private const string GetHistoryUrl = "/dashboard/notes/get-history";
    private const string CreateFolderUrl = "/dashboard/notes/create-folder";
    private const string CreateDocumentUrl = "/dashboard/notes/create-document";
    private const string UpdateDocumentUrl = "/dashboard/notes/update-document";
    private const string UpdateContentUrl = "/dashboard/notes/update-content";
    private const string RenameNodeUrl = "/dashboard/notes/rename-node";
    private const string MoveNodeUrl = "/dashboard/notes/move-node";
    private const string ArchiveNodeUrl = "/dashboard/notes/archive-node";
    private const string GetLinkedNotesUrl = "/dashboard/notes/get-linked-notes";
    private const string CreateLinkUrl = "/dashboard/notes/create-link";
    private const string DeleteLinkUrl = "/dashboard/notes/delete-link";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITaskSeeder _taskSeeder;

    public NotesTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task GetTreeRequiresWorkspaceHeader()
    {
        var response = await PostWithoutWorkspaceHeaderAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTreeRejectsInvalidWorkspaceHeader()
    {
        var response = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTreeRejectsWorkspaceUserRole()
    {
        await CreateDocumentAsync("Owner.md", "# Owner", visibility: NoteVisibility.Workspace);
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(GetTreeUrl, userToken, new GetNotesTreeRequest(), _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFolderCreatesRootFolder()
    {
        var folder = await CreateFolderAsync("Clients");
        var tree = await GetTreeAsync();

        Assert.Contains(tree.Nodes, item =>
            item.Id == folder.Id
            && item.ParentId == null
            && item.Type == NoteNodeType.Folder
            && item.Title == "Clients"
        );
    }

    [Fact]
    public async Task CreateDocumentCreatesRootDocument()
    {
        var response = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            ParentId = null,
            Title = "Deploy.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var document = await response.GetJsonDataAsync<NoteDocumentDto>();
        await UpdateContentAsync(document.Id, "# Deploy notes");
        var treeResponse = await PostRequestAsync(GetTreeUrl, _jwtToken, new GetNotesTreeRequest(), _workspace.Id);
        var rawTreeJson = await treeResponse.GetDataAsStringAsync();
        treeResponse.EnsureSuccessStatusCode();
        var tree = await treeResponse.GetJsonDataAsync<GetNotesTreeResponse>();
        var loadedDocument = await GetDocumentAsync(document.Id);

        Assert.Contains(tree.Nodes, item => item.Id == document.Id && item.Type == NoteNodeType.Document);
        Assert.DoesNotContain("markdownContent", rawTreeJson);
        Assert.NotNull(loadedDocument.LastContentId);
        var loadedContent = await GetContentAsync(loadedDocument.LastContentId!.Value);
        Assert.Equal("# Deploy notes", loadedContent.MarkdownContent);
    }

    [Fact]
    public async Task CreateDocumentUnderFolder()
    {
        var folder = await CreateFolderAsync("Work");
        var document = await CreateDocumentAsync("Deploy.md", "# Deploy", folder.Id);
        var tree = await GetTreeAsync();

        Assert.Contains(tree.Nodes, item => item.Id == document.Id && item.ParentId == folder.Id);
    }

    [Fact]
    public async Task CreateDocumentRejectsDocumentParent()
    {
        var document = await CreateDocumentAsync("Parent.md", "# Parent");
        var response = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            ParentId = document.Id,
            Title = "Child.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDocumentRejectsCrossWorkspaceNoteAccess()
    {
        var document = await CreateDocumentAsync("Workspace A.md", "# A");
        var (_, _, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var response = await PostRequestAsync(GetDocumentUrl, _jwtToken, new GetNoteDocumentRequest
        {
            NoteId = document.Id
        }, otherWorkspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDocumentAndContentAreUpdatedSeparately()
    {
        var document = await CreateDocumentAsync("Deploy.md", "# Deploy");
        var response = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = document.Id,
            Title = "Deploy updated.md",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        await UpdateContentAsync(document.Id, "# Updated");
        var actual = await GetDocumentAsync(document.Id);

        Assert.Equal("Deploy updated.md", actual.Title);
        Assert.NotNull(actual.LastContentId);
        var content = await GetContentAsync(actual.LastContentId!.Value);
        Assert.Equal("# Updated", content.MarkdownContent);
        Assert.NotNull(actual.UpdatedAt);
    }

    [Fact]
    public async Task GetHistoryReturnsDocumentSnapshots()
    {
        var document = await CreateDocumentAsync("Deploy.md", "# Deploy");
        await UpdateDocumentAsync(document.Id, "Deploy updated.md", "# Updated", NoteVisibility.Workspace);
        var response = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = document.Id,
            SortOrder = 250
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var history = await GetHistoryAsync(document.Id);

        Assert.Equal(5, history.History.Count);
        Assert.Collection(
            history.History,
            item =>
            {
                Assert.Equal("Deploy.md", item.Title);
                Assert.Equal(string.Empty, item.MarkdownContent);
                Assert.Equal(1000, item.SortOrder);
            },
            item =>
            {
                Assert.Equal("Deploy.md", item.Title);
                Assert.Equal("# Deploy", item.MarkdownContent);
                Assert.Equal(1000, item.SortOrder);
            },
            item =>
            {
                Assert.Equal("Deploy updated.md", item.Title);
                Assert.Equal("# Deploy", item.MarkdownContent);
                Assert.Equal(1000, item.SortOrder);
            },
            item =>
            {
                Assert.Equal("Deploy updated.md", item.Title);
                Assert.Equal("# Updated", item.MarkdownContent);
                Assert.Equal(1000, item.SortOrder);
            },
            item =>
            {
                Assert.Equal("Deploy updated.md", item.Title);
                Assert.Equal("# Updated", item.MarkdownContent);
                Assert.Equal(250, item.SortOrder);
            }
        );
    }

    [Fact]
    public async Task GetHistoryRejectsFolder()
    {
        var folder = await CreateFolderAsync("Folder");
        var response = await PostRequestAsync(GetHistoryUrl, _jwtToken, new GetNoteNodeHistoryRequest
        {
            NoteId = folder.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SavingFolderDoesNotCreateHistory()
    {
        var folder = await CreateFolderAsync("Folder");
        await RenameNodeAsync(folder.Id, "Renamed folder");
        await FlushDbChanges(isClearSession: true);
        var historyCount = await DbSessionProvider.CurrentSession
            .Query<NoteNodeHistoryEntity>()
            .CountAsync();

        Assert.Equal(0, historyCount);
    }

    [Fact]
    public async Task UpdateDocumentRejectsFolder()
    {
        var folder = await CreateFolderAsync("Folder");
        var response = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = folder.Id,
            Title = "Folder",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RenameNodeWorksForFolderAndDocument()
    {
        var folder = await CreateFolderAsync("Folder");
        var document = await CreateDocumentAsync("Document.md", "# Document");

        await RenameNodeAsync(folder.Id, "Renamed folder");
        await RenameNodeAsync(document.Id, "Renamed document.md");
        var tree = await GetTreeAsync();

        Assert.Contains(tree.Nodes, item => item.Id == folder.Id && item.Title == "Renamed folder");
        Assert.Contains(tree.Nodes, item => item.Id == document.Id && item.Title == "Renamed document.md");
    }

    [Fact]
    public async Task MoveNodeMovesDocumentToAnotherFolder()
    {
        var folderA = await CreateFolderAsync("Folder A");
        var folderB = await CreateFolderAsync("Folder B");
        var document = await CreateDocumentAsync("Document.md", "# Document", folderA.Id);

        var response = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = document.Id,
            ParentId = folderB.Id
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var tree = await GetTreeAsync();

        Assert.Contains(tree.Nodes, item => item.Id == document.Id && item.ParentId == folderB.Id);
    }

    [Fact]
    public async Task MoveNodeRejectsMovingFolderIntoItself()
    {
        var folder = await CreateFolderAsync("Folder");
        var response = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = folder.Id,
            ParentId = folder.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveNodeRejectsMovingFolderIntoDescendant()
    {
        var parent = await CreateFolderAsync("Parent");
        var child = await CreateFolderAsync("Child", parent.Id);
        var response = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = parent.Id,
            ParentId = child.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveNodeRemovesDocumentFromTree()
    {
        var document = await CreateDocumentAsync("Archive.md", "# Archive");
        var response = await PostRequestAsync(ArchiveNodeUrl, _jwtToken, new ArchiveNoteNodeRequest
        {
            NoteId = document.Id
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var tree = await GetTreeAsync();

        Assert.DoesNotContain(tree.Nodes, item => item.Id == document.Id);
    }

    [Fact]
    public async Task ArchiveNodeHandlesFolderChildren()
    {
        var folder = await CreateFolderAsync("Folder");
        var child = await CreateDocumentAsync("Child.md", "# Child", folder.Id);
        var response = await PostRequestAsync(ArchiveNodeUrl, _jwtToken, new ArchiveNoteNodeRequest
        {
            NoteId = folder.Id
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var tree = await GetTreeAsync();

        Assert.DoesNotContain(tree.Nodes, item => item.Id == folder.Id);
        Assert.DoesNotContain(tree.Nodes, item => item.Id == child.Id);
    }

    [Fact]
    public async Task CreateLinkLinksNoteToProject()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var document = await CreateDocumentAsync("Project.md", "# Project");
        var response = await CreateLinkAsync(document.Id, project.Id);
        var linkedNotes = await GetLinkedNotesAsync(project.Id);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Contains(linkedNotes.Notes, item => item.Id == document.Id);
    }

    [Fact]
    public async Task CreateLinkRejectsDuplicateLink()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var document = await CreateDocumentAsync("Project.md", "# Project");
        await CreateLinkAsync(document.Id, project.Id);
        var response = await PostRequestAsync(CreateLinkUrl, _jwtToken, new CreateNoteLinkRequest
        {
            NoteId = document.Id,
            EntityType = NoteLinkEntityType.Project,
            EntityId = project.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLinkRejectsCrossWorkspaceProject()
    {
        var (_, _, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProject = await _projectSeeder.CreateAsync(otherWorkspace);
        var document = await CreateDocumentAsync("Project.md", "# Project");
        var response = await PostRequestAsync(CreateLinkUrl, _jwtToken, new CreateNoteLinkRequest
        {
            NoteId = document.Id,
            EntityType = NoteLinkEntityType.Project,
            EntityId = otherProject.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLinkRemovesLink()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var document = await CreateDocumentAsync("Project.md", "# Project");
        var link = await CreateLinkAsync(document.Id, project.Id);
        var response = await PostRequestAsync(DeleteLinkUrl, _jwtToken, new DeleteNoteLinkRequest
        {
            NoteId = document.Id,
            LinkId = link.Id
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var linkedNotes = await GetLinkedNotesAsync(project.Id);

        Assert.DoesNotContain(linkedNotes.Notes, item => item.Id == document.Id);
    }

    [Fact]
    public async Task GetLinkedNotesRespectsPrivateVisibility()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var privateDocument = await CreateDocumentAsync("Private.md", "# Private", visibility: NoteVisibility.Private);
        await CreateLinkAsync(privateDocument.Id, project.Id);
        var (managerToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        var managerLinkedNotes = await GetLinkedNotesAsync(project.Id, managerToken);

        Assert.DoesNotContain(managerLinkedNotes.Notes, item => item.Id == privateDocument.Id);

        var workspaceDocument = await CreateDocumentAsync("Workspace.md", "# Workspace", visibility: NoteVisibility.Workspace);
        await CreateLinkAsync(workspaceDocument.Id, project.Id);
        managerLinkedNotes = await GetLinkedNotesAsync(project.Id, managerToken);

        Assert.Contains(managerLinkedNotes.Notes, item => item.Id == workspaceDocument.Id);
    }

    [Fact]
    public async Task WorkspaceIsolationInGetTree()
    {
        var workspaceANote = await CreateDocumentAsync("A.md", "# A");
        var (otherToken, _, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var workspaceBNote = await CreateDocumentAsync(
            "B.md",
            "# B",
            token: otherToken,
            workspaceId: otherWorkspace.Id
        );
        var tree = await GetTreeAsync();

        Assert.Contains(tree.Nodes, item => item.Id == workspaceANote.Id);
        Assert.DoesNotContain(tree.Nodes, item => item.Id == workspaceBNote.Id);
    }

    [Fact]
    public void AllNotesEndpointsArePostOnly()
    {
        var httpMethodAttributes = typeof(NotesController)
            .GetMethods()
            .SelectMany(item => item.GetCustomAttributes(typeof(HttpMethodAttribute), false))
            .Cast<HttpMethodAttribute>()
            .ToList();

        Assert.NotEmpty(httpMethodAttributes);
        Assert.All(httpMethodAttributes, item => Assert.Equal("POST", Assert.Single(item.HttpMethods)));
    }

    [Fact]
    public async Task CreateDocumentCanLinkToClientProjectAndTask()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var task = await _taskSeeder.CreateAsync(taskList, _user);
        var response = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "Linked.md",
            Visibility = NoteVisibility.Workspace,
            Links =
            [
                new NoteLinkRequestDto
                {
                    EntityType = NoteLinkEntityType.Client,
                    EntityId = project.Client.Id
                },
                new NoteLinkRequestDto
                {
                    EntityType = NoteLinkEntityType.Project,
                    EntityId = project.Id
                },
                new NoteLinkRequestDto
                {
                    EntityType = NoteLinkEntityType.Task,
                    EntityId = task.Id
                }
            ]
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var document = await response.GetJsonDataAsync<NoteDocumentDto>();

        Assert.Equal(3, document.Links.Count);
    }

    private async Task<HttpResponseMessage> PostWithoutWorkspaceHeaderAsync(string url, string jwtToken, object data)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        HttpClient.DefaultRequestHeaders.Remove(AuthConstants.WorkspaceIdHeaderName);
        return await HttpClient.PostAsync(url, JsonContent.Create(data));
    }

    private async Task<NoteTreeNodeDto> CreateFolderAsync(
        string title,
        Guid? parentId = null,
        string? token = null,
        Guid? workspaceId = null
    )
    {
        var response = await PostRequestAsync(CreateFolderUrl, token ?? _jwtToken, new CreateNoteFolderRequest
        {
            ParentId = parentId,
            Title = title,
            Visibility = NoteVisibility.Private
        }, workspaceId ?? _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteTreeNodeDto>();
    }

    private async Task<NoteDocumentDto> CreateDocumentAsync(
        string title,
        string markdownContent,
        Guid? parentId = null,
        NoteVisibility visibility = NoteVisibility.Private,
        string? token = null,
        Guid? workspaceId = null
    )
    {
        var response = await PostRequestAsync(CreateDocumentUrl, token ?? _jwtToken, new CreateNoteDocumentRequest
        {
            ParentId = parentId,
            Title = title,
            Visibility = visibility
        }, workspaceId ?? _workspace.Id);
        response.EnsureSuccessStatusCode();
        var document = await response.GetJsonDataAsync<NoteDocumentDto>();
        await UpdateContentAsync(document.Id, markdownContent, token, workspaceId);
        return await GetDocumentAsync(document.Id, token, workspaceId);
    }

    private async Task<GetNotesTreeResponse> GetTreeAsync(string? token = null, Guid? workspaceId = null)
    {
        var response = await PostRequestAsync(GetTreeUrl, token ?? _jwtToken, new GetNotesTreeRequest
        {
            IncludeArchived = false
        }, workspaceId ?? _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<GetNotesTreeResponse>();
    }

    private async Task<NoteDocumentDto> GetDocumentAsync(Guid noteId, string? token = null, Guid? workspaceId = null)
    {
        var response = await PostRequestAsync(GetDocumentUrl, token ?? _jwtToken, new GetNoteDocumentRequest
        {
            NoteId = noteId
        }, workspaceId ?? _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteDocumentDto>();
    }

    private async Task<NoteContentDto> GetContentAsync(Guid contentId)
    {
        var response = await PostRequestAsync(GetContentUrl, _jwtToken, new GetNoteContentRequest
        {
            ContentId = contentId
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteContentDto>();
    }

    private async Task<GetNoteNodeHistoryResponse> GetHistoryAsync(Guid noteId)
    {
        var response = await PostRequestAsync(GetHistoryUrl, _jwtToken, new GetNoteNodeHistoryRequest
        {
            NoteId = noteId
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<GetNoteNodeHistoryResponse>();
    }

    private async Task UpdateDocumentAsync(
        Guid noteId,
        string title,
        string markdownContent,
        NoteVisibility visibility
    )
    {
        var response = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = noteId,
            Title = title,
            Visibility = visibility
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        await UpdateContentAsync(noteId, markdownContent);
    }

    private async Task<NoteContentDto> UpdateContentAsync(
        Guid noteId,
        string markdownContent,
        string? token = null,
        Guid? workspaceId = null
    )
    {
        var response = await PostRequestAsync(UpdateContentUrl, token ?? _jwtToken, new UpdateNoteContentRequest
        {
            NoteId = noteId,
            MarkdownContent = markdownContent
        }, workspaceId ?? _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteContentDto>();
    }

    private async System.Threading.Tasks.Task RenameNodeAsync(Guid noteId, string title)
    {
        var response = await PostRequestAsync(RenameNodeUrl, _jwtToken, new RenameNoteNodeRequest
        {
            NoteId = noteId,
            Title = title
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
    }

    private async Task<NoteLinkDto> CreateLinkAsync(Guid noteId, Guid projectId)
    {
        var response = await PostRequestAsync(CreateLinkUrl, _jwtToken, new CreateNoteLinkRequest
        {
            NoteId = noteId,
            EntityType = NoteLinkEntityType.Project,
            EntityId = projectId
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<NoteLinkDto>();
    }

    private async Task<GetLinkedNotesResponse> GetLinkedNotesAsync(Guid projectId, string? token = null)
    {
        var response = await PostRequestAsync(GetLinkedNotesUrl, token ?? _jwtToken, new GetLinkedNotesRequest
        {
            EntityType = NoteLinkEntityType.Project,
            EntityId = projectId
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        return await response.GetJsonDataAsync<GetLinkedNotesResponse>();
    }

}
