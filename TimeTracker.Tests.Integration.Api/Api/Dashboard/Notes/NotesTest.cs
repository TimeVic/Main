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
    public async Task GetTreeReturnsSharedNotesAndTheirParentFoldersForWorkspaceUser()
    {
        var folder = await CreateFolderAsync("Shared folder", visibility: NoteVisibility.Workspace);
        var document = await CreateDocumentAsync(
            "Owner.md",
            "# Owner",
            folder.Id,
            NoteVisibility.Workspace
        );
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(GetTreeUrl, userToken, new GetNotesTreeRequest
        {
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();
        var tree = await response.GetJsonDataAsync<GetNotesTreeResponse>();

        Assert.Contains(tree.Nodes, item => item.Id == folder.Id);
        Assert.Contains(tree.Nodes, item => item.Id == document.Id && item.ParentId == folder.Id);
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
            Title = "Deploy updated.md"
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
        await UpdateDocumentAsync(document.Id, "Deploy updated.md", "# Updated");
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
            Title = "Folder"
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

    [Fact]
    public async Task CreateDocumentRejectsParentWithDifferentVisibility()
    {
        var privateFolder = await CreateFolderAsync("Private Folder", visibility: NoteVisibility.Private);
        var responseWorkspaceUnderPrivate = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            ParentId = privateFolder.Id,
            Title = "WorkspaceInPrivate.md",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responseWorkspaceUnderPrivate.StatusCode);

        var workspaceFolder = await CreateFolderAsync("Workspace Folder", visibility: NoteVisibility.Workspace);
        var responsePrivateUnderWorkspace = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            ParentId = workspaceFolder.Id,
            Title = "PrivateInWorkspace.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responsePrivateUnderWorkspace.StatusCode);
    }

    [Fact]
    public async Task CreateFolderRejectsParentWithDifferentVisibility()
    {
        var privateFolder = await CreateFolderAsync("Private Folder", visibility: NoteVisibility.Private);
        var responseWorkspaceUnderPrivate = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            ParentId = privateFolder.Id,
            Title = "Subfolder",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responseWorkspaceUnderPrivate.StatusCode);

        var workspaceFolder = await CreateFolderAsync("Workspace Folder", visibility: NoteVisibility.Workspace);
        var responsePrivateUnderWorkspace = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            ParentId = workspaceFolder.Id,
            Title = "Subfolder",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responsePrivateUnderWorkspace.StatusCode);
    }

    [Fact]
    public async Task MoveNodeRejectsMovingIntoParentWithDifferentVisibility()
    {
        var privateFolder = await CreateFolderAsync("Private Folder", visibility: NoteVisibility.Private);
        var workspaceFolder = await CreateFolderAsync("Workspace Folder", visibility: NoteVisibility.Workspace);
        var privateDoc = await CreateDocumentAsync("Private.md", "# Private", visibility: NoteVisibility.Private);
        var workspaceDoc = await CreateDocumentAsync("Workspace.md", "# Workspace", visibility: NoteVisibility.Workspace);

        var movePrivateToWorkspaceResponse = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = privateDoc.Id,
            ParentId = workspaceFolder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, movePrivateToWorkspaceResponse.StatusCode);

        var moveWorkspaceToPrivateResponse = await PostRequestAsync(MoveNodeUrl, _jwtToken, new MoveNoteNodeRequest
        {
            NoteId = workspaceDoc.Id,
            ParentId = privateFolder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, moveWorkspaceToPrivateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateDocumentPreservesVisibility()
    {
        var workspaceFolder = await CreateFolderAsync("Workspace Folder", visibility: NoteVisibility.Workspace);
        var workspaceDoc = await CreateDocumentAsync("Doc.md", "# Content", workspaceFolder.Id, NoteVisibility.Workspace);

        var updateResponse = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = workspaceDoc.Id,
            Title = "UpdatedDoc.md"
        }, _workspace.Id);
        updateResponse.EnsureSuccessStatusCode();

        var updatedDoc = await updateResponse.GetJsonDataAsync<NoteDocumentDto>();
        Assert.Equal(NoteVisibility.Workspace, updatedDoc.Visibility);
        Assert.Equal("UpdatedDoc.md", updatedDoc.Title);
    }

    [Fact]
    public async Task WorkspaceUserCanCreateAndManageOwnPrivateNotes()
    {
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var folder = await CreateFolderAsync("My Private Folder", token: userToken, visibility: NoteVisibility.Private);
        var document = await CreateDocumentAsync(
            "My Note.md",
            "# My Private Content",
            folder.Id,
            NoteVisibility.Private,
            userToken
        );

        var loadedDocument = await GetDocumentAsync(document.Id, userToken);
        Assert.Equal("My Note.md", loadedDocument.Title);

        var userTree = await GetTreeAsync(userToken);
        Assert.Contains(userTree.Nodes, item => item.Id == folder.Id);
        Assert.Contains(userTree.Nodes, item => item.Id == document.Id);
    }

    [Fact]
    public async Task WorkspaceUserCannotCreateWorkspaceDocumentOrFolder()
    {
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var createFolderResponse = await PostRequestAsync(CreateFolderUrl, userToken, new CreateNoteFolderRequest
        {
            Title = "Workspace Folder",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, createFolderResponse.StatusCode);

        var createDocResponse = await PostRequestAsync(CreateDocumentUrl, userToken, new CreateNoteDocumentRequest
        {
            Title = "Workspace Doc.md",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, createDocResponse.StatusCode);
    }

    [Fact]
    public async Task WorkspaceUserCanUpdateWorkspaceDocumentAndContentInTeamMode()
    {
        _workspace.Mode = WorkspaceMode.Team;
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var workspaceFolder = await CreateFolderAsync("Shared Folder", visibility: NoteVisibility.Workspace);
        var workspaceDoc = await CreateDocumentAsync("Shared Doc.md", "# Shared", workspaceFolder.Id, NoteVisibility.Workspace);

        // Update document (Title)
        var updateDocResponse = await PostRequestAsync(UpdateDocumentUrl, userToken, new UpdateNoteDocumentRequest
        {
            NoteId = workspaceDoc.Id,
            Title = "Updated Shared Doc.md"
        }, _workspace.Id);
        updateDocResponse.EnsureSuccessStatusCode();

        var updatedDoc = await updateDocResponse.GetJsonDataAsync<NoteDocumentDto>();
        Assert.NotNull(updatedDoc);
        Assert.Equal("Updated Shared Doc.md", updatedDoc.Title);

        // Update content (Markdown)
        var updateContentResponse = await PostRequestAsync(UpdateContentUrl, userToken, new UpdateNoteContentRequest
        {
            NoteId = workspaceDoc.Id,
            MarkdownContent = "# User updated content"
        }, _workspace.Id);
        updateContentResponse.EnsureSuccessStatusCode();

        var updatedContent = await updateContentResponse.GetJsonDataAsync<NoteContentDto>();
        Assert.NotNull(updatedContent);
        Assert.Equal("# User updated content", updatedContent.MarkdownContent);
    }

    [Fact]
    public async Task WorkspaceUserCannotCreateOrArchiveOrMoveWorkspaceDocumentOrFolderInTeamMode()
    {
        _workspace.Mode = WorkspaceMode.Team;
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var workspaceFolder = await CreateFolderAsync("Shared Folder", visibility: NoteVisibility.Workspace);
        var workspaceDoc = await CreateDocumentAsync("Shared Doc.md", "# Shared", workspaceFolder.Id, NoteVisibility.Workspace);

        // Rename folder
        var renameResponse = await PostRequestAsync(RenameNodeUrl, userToken, new RenameNoteNodeRequest
        {
            NoteId = workspaceFolder.Id,
            Title = "Renamed Folder"
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, renameResponse.StatusCode);

        // Move
        var moveResponse = await PostRequestAsync(MoveNodeUrl, userToken, new MoveNoteNodeRequest
        {
            NoteId = workspaceDoc.Id,
            ParentId = null
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, moveResponse.StatusCode);

        // Archive document
        var archiveDocResponse = await PostRequestAsync(ArchiveNodeUrl, userToken, new ArchiveNoteNodeRequest
        {
            NoteId = workspaceDoc.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, archiveDocResponse.StatusCode);

        // Archive folder
        var archiveFolderResponse = await PostRequestAsync(ArchiveNodeUrl, userToken, new ArchiveNoteNodeRequest
        {
            NoteId = workspaceFolder.Id
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, archiveFolderResponse.StatusCode);
    }

    [Fact]
    public async Task WorkspaceManagerCanUpdateWorkspaceDocumentAndContent()
    {
        var (managerToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );

        var workspaceFolder = await CreateFolderAsync("Shared Folder", visibility: NoteVisibility.Workspace);
        var workspaceDoc = await CreateDocumentAsync("Shared Doc.md", "# Shared", workspaceFolder.Id, NoteVisibility.Workspace);

        // Update document
        var updateDocResponse = await PostRequestAsync(UpdateDocumentUrl, managerToken, new UpdateNoteDocumentRequest
        {
            NoteId = workspaceDoc.Id,
            Title = "Updated Shared Doc.md"
        }, _workspace.Id);
        updateDocResponse.EnsureSuccessStatusCode();

        // Update content
        var updateContentResponse = await PostRequestAsync(UpdateContentUrl, managerToken, new UpdateNoteContentRequest
        {
            NoteId = workspaceDoc.Id,
            MarkdownContent = "# Manager updated content"
        }, _workspace.Id);
        updateContentResponse.EnsureSuccessStatusCode();

        // Rename
        var renameResponse = await PostRequestAsync(RenameNodeUrl, managerToken, new RenameNoteNodeRequest
        {
            NoteId = workspaceFolder.Id,
            Title = "Manager Renamed Folder"
        }, _workspace.Id);
        renameResponse.EnsureSuccessStatusCode();

        // Archive
        var archiveResponse = await PostRequestAsync(ArchiveNodeUrl, managerToken, new ArchiveNoteNodeRequest
        {
            NoteId = workspaceDoc.Id
        }, _workspace.Id);
        archiveResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task WorkspaceUserCanUpdateOwnPrivateDocumentAndFolder()
    {
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var privateFolder = await CreateFolderAsync("User Folder", token: userToken, visibility: NoteVisibility.Private);
        var privateDoc = await CreateDocumentAsync("User Doc.md", "# Private", privateFolder.Id, NoteVisibility.Private, token: userToken);

        // Update document
        var updateDocResponse = await PostRequestAsync(UpdateDocumentUrl, userToken, new UpdateNoteDocumentRequest
        {
            NoteId = privateDoc.Id,
            Title = "Updated User Doc.md"
        }, _workspace.Id);
        updateDocResponse.EnsureSuccessStatusCode();

        // Update content
        var updateContentResponse = await PostRequestAsync(UpdateContentUrl, userToken, new UpdateNoteContentRequest
        {
            NoteId = privateDoc.Id,
            MarkdownContent = "# Updated user content"
        }, _workspace.Id);
        updateContentResponse.EnsureSuccessStatusCode();

        // Rename
        var renameResponse = await PostRequestAsync(RenameNodeUrl, userToken, new RenameNoteNodeRequest
        {
            NoteId = privateFolder.Id,
            Title = "Renamed User Folder"
        }, _workspace.Id);
        renameResponse.EnsureSuccessStatusCode();

        // Archive
        var archiveResponse = await PostRequestAsync(ArchiveNodeUrl, userToken, new ArchiveNoteNodeRequest
        {
            NoteId = privateDoc.Id
        }, _workspace.Id);
        archiveResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task WorkspaceUserCannotAccessOtherMembersPrivateDocument()
    {
        var (userAToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var (userBToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);

        var privateDocA = await CreateDocumentAsync(
            "UserA.md",
            "# User A Private",
            visibility: NoteVisibility.Private,
            token: userAToken
        );

        var response = await PostRequestAsync(GetDocumentUrl, userBToken, new GetNoteDocumentRequest
        {
            NoteId = privateDocA.Id
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTreeIsolatesPrivateNotesBetweenDifferentUsers()
    {
        var (userAToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);
        var (userBToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);

        var privateDocA = await CreateDocumentAsync(
            "UserA.md",
            "# User A",
            visibility: NoteVisibility.Private,
            token: userAToken
        );
        var privateDocB = await CreateDocumentAsync(
            "UserB.md",
            "# User B",
            visibility: NoteVisibility.Private,
            token: userBToken
        );
        var workspaceDoc = await CreateDocumentAsync(
            "Shared.md",
            "# Shared",
            visibility: NoteVisibility.Workspace
        );

        var treeA = await GetTreeAsync(userAToken);
        var treeB = await GetTreeAsync(userBToken);
        var workspaceTreeA = await GetTreeAsync(userAToken, visibility: NoteVisibility.Workspace);

        Assert.Contains(treeA.Nodes, item => item.Id == privateDocA.Id);
        Assert.DoesNotContain(treeA.Nodes, item => item.Id == privateDocB.Id);
        Assert.DoesNotContain(treeA.Nodes, item => item.Id == workspaceDoc.Id);

        Assert.Contains(treeB.Nodes, item => item.Id == privateDocB.Id);
        Assert.DoesNotContain(treeB.Nodes, item => item.Id == privateDocA.Id);
        Assert.DoesNotContain(treeB.Nodes, item => item.Id == workspaceDoc.Id);

        Assert.Contains(workspaceTreeA.Nodes, item => item.Id == workspaceDoc.Id);
        Assert.DoesNotContain(workspaceTreeA.Nodes, item => item.Id == privateDocA.Id);
        Assert.DoesNotContain(workspaceTreeA.Nodes, item => item.Id == privateDocB.Id);
    }

    private async Task<NoteTreeNodeDto> CreateFolderAsync(
        string title,
        Guid? parentId = null,
        string? token = null,
        Guid? workspaceId = null,
        NoteVisibility visibility = NoteVisibility.Private
    )
    {
        var response = await PostRequestAsync(CreateFolderUrl, token ?? _jwtToken, new CreateNoteFolderRequest
        {
            ParentId = parentId,
            Title = title,
            Visibility = visibility
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

    private async Task<GetNotesTreeResponse> GetTreeAsync(
        string? token = null,
        Guid? workspaceId = null,
        NoteVisibility? visibility = null
    )
    {
        var response = await PostRequestAsync(GetTreeUrl, token ?? _jwtToken, new GetNotesTreeRequest
        {
            IncludeArchived = false,
            Visibility = visibility
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
        string markdownContent
    )
    {
        var response = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = noteId,
            Title = title
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

    [Fact]
    public async Task SoloWorkspaceCannotCreateWorkspaceDocument()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var responseWorkspace = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "WorkspaceInSolo.md",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responseWorkspace.StatusCode);

        var responsePrivate = await PostRequestAsync(CreateDocumentUrl, _jwtToken, new CreateNoteDocumentRequest
        {
            Title = "PrivateInSolo.md",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        responsePrivate.EnsureSuccessStatusCode();
        var document = await responsePrivate.GetJsonDataAsync<NoteDocumentDto>();
        Assert.Equal(NoteVisibility.Private, document.Visibility);
        Assert.Equal("PrivateInSolo.md", document.Title);
    }

    [Fact]
    public async Task SoloWorkspaceCannotCreateWorkspaceFolder()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var responseWorkspace = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            Title = "WorkspaceFolderInSolo",
            Visibility = NoteVisibility.Workspace
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, responseWorkspace.StatusCode);

        var responsePrivate = await PostRequestAsync(CreateFolderUrl, _jwtToken, new CreateNoteFolderRequest
        {
            Title = "PrivateFolderInSolo",
            Visibility = NoteVisibility.Private
        }, _workspace.Id);

        responsePrivate.EnsureSuccessStatusCode();
        var folder = await responsePrivate.GetJsonDataAsync<NoteTreeNodeDto>();
        Assert.Equal(NoteVisibility.Private, folder.Visibility);
        Assert.Equal("PrivateFolderInSolo", folder.Title);
    }

    [Fact]
    public async Task SoloWorkspaceUpdateDocumentPreservesPrivateVisibility()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var privateDoc = await CreateDocumentAsync("PrivateInSolo.md", "# Private", visibility: NoteVisibility.Private);

        var response = await PostRequestAsync(UpdateDocumentUrl, _jwtToken, new UpdateNoteDocumentRequest
        {
            NoteId = privateDoc.Id,
            Title = "UpdatedTitle.md"
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var updatedDoc = await response.GetJsonDataAsync<NoteDocumentDto>();
        Assert.Equal(NoteVisibility.Private, updatedDoc.Visibility);
        Assert.Equal("UpdatedTitle.md", updatedDoc.Title);
    }

    [Fact]
    public async Task GetTreeFiltersByWorkspaceAndPrivateVisibility()
    {
        var workspaceDoc = await CreateDocumentAsync("SharedDoc.md", "# Shared", visibility: NoteVisibility.Workspace);
        var privateDoc = await CreateDocumentAsync("PrivateDoc.md", "# Private", visibility: NoteVisibility.Private);

        // When filtering by Workspace visibility
        var workspaceTree = await GetTreeAsync(visibility: NoteVisibility.Workspace);
        Assert.Contains(workspaceTree.Nodes, item => item.Id == workspaceDoc.Id);
        Assert.DoesNotContain(workspaceTree.Nodes, item => item.Id == privateDoc.Id);

        // When filtering by Private visibility
        var privateTree = await GetTreeAsync(visibility: NoteVisibility.Private);
        Assert.Contains(privateTree.Nodes, item => item.Id == privateDoc.Id);
        Assert.DoesNotContain(privateTree.Nodes, item => item.Id == workspaceDoc.Id);

        // When no visibility specified, defaults to Private
        var defaultTree = await GetTreeAsync(visibility: null);
        Assert.Contains(defaultTree.Nodes, item => item.Id == privateDoc.Id);
        Assert.DoesNotContain(defaultTree.Nodes, item => item.Id == workspaceDoc.Id);
    }

    [Fact]
    public async Task SoloWorkspaceGetTreeReturnsPrivateNotesWhenVisibilityIsNull()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var privateDoc = await CreateDocumentAsync("SoloPrivateDoc.md", "# Private in Solo", visibility: NoteVisibility.Private);

        var tree = await GetTreeAsync(visibility: null);
        Assert.Contains(tree.Nodes, item => item.Id == privateDoc.Id);
    }

    [Fact]
    public async Task GetTreeWithFolderHierarchyFiltersByVisibility()
    {
        var workspaceFolder = await CreateFolderAsync("SharedFolder", visibility: NoteVisibility.Workspace);
        var workspaceDoc = await CreateDocumentAsync("SharedInFolder.md", "# Shared", parentId: workspaceFolder.Id, visibility: NoteVisibility.Workspace);

        var privateFolder = await CreateFolderAsync("PrivateFolder", visibility: NoteVisibility.Private);
        var privateDoc = await CreateDocumentAsync("PrivateInFolder.md", "# Private", parentId: privateFolder.Id, visibility: NoteVisibility.Private);

        var workspaceTree = await GetTreeAsync(visibility: NoteVisibility.Workspace);
        Assert.Contains(workspaceTree.Nodes, item => item.Id == workspaceFolder.Id);
        Assert.Contains(workspaceTree.Nodes, item => item.Id == workspaceDoc.Id);
        Assert.DoesNotContain(workspaceTree.Nodes, item => item.Id == privateFolder.Id);
        Assert.DoesNotContain(workspaceTree.Nodes, item => item.Id == privateDoc.Id);

        var privateTree = await GetTreeAsync(visibility: NoteVisibility.Private);
        Assert.Contains(privateTree.Nodes, item => item.Id == privateFolder.Id);
        Assert.Contains(privateTree.Nodes, item => item.Id == privateDoc.Id);
        Assert.DoesNotContain(privateTree.Nodes, item => item.Id == workspaceFolder.Id);
        Assert.DoesNotContain(privateTree.Nodes, item => item.Id == workspaceDoc.Id);
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
