using AutoMapper;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public abstract class NoteRequestHandlerBase
{
    protected class WorkspaceContext
    {
        public required UserEntity User { get; set; }

        public required WorkspaceEntity Workspace { get; set; }
    }

    protected const int TitleMaxLength = 200;
    protected const int MarkdownMaxLength = 5_000_000;

    protected readonly IMapper Mapper;
    protected readonly IApiRequestService ApiRequestService;
    protected readonly IUserDao UserDao;
    protected readonly ISecurityManager SecurityManager;
    protected readonly INoteDao NoteDao;
    protected readonly IClientDao ClientDao;
    protected readonly IProjectDao ProjectDao;
    protected readonly ITaskDao TaskDao;

    protected NoteRequestHandlerBase(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteDao noteDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskDao taskDao
    )
    {
        Mapper = mapper;
        ApiRequestService = apiRequestService;
        UserDao = userDao;
        SecurityManager = securityManager;
        NoteDao = noteDao;
        ClientDao = clientDao;
        ProjectDao = projectDao;
        TaskDao = taskDao;
    }

    protected async Task<WorkspaceContext> GetWorkspaceContextAsync(AccessLevel accessLevel = AccessLevel.Write)
    {
        var user = await ApiRequestService.GetCurrentUser();
        var workspaceId = ApiRequestService.GetCurrentWorkspaceId();
        if (!workspaceId.HasValue)
        {
            throw new RecordNotFoundException("Workspace not found");
        }

        var workspace = await UserDao.GetUsersWorkspace(user!, workspaceId);
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        if (!await SecurityManager.HasAccess(accessLevel, user, workspace))
        {
            throw new HasNoAccessException();
        }

        return new WorkspaceContext
        {
            User = user,
            Workspace = workspace
        };
    }

    protected async Task<NoteNodeEntity> GetNoteAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid noteId,
        AccessLevel accessLevel
    )
    {
        var note = await NoteDao.GetNodeByIdAsync(workspace, noteId);
        RecordNotFoundException.ThrowIfNull(note);
        await SecurityManager.CheckAccess(accessLevel, user, note);
        return note;
    }

    protected async Task<ICollection<NoteNodeEntity>> GetAvailableNotesAsync(
        UserEntity user,
        IEnumerable<NoteNodeEntity> notes,
        AccessLevel accessLevel
    )
    {
        var result = new List<NoteNodeEntity>();
        foreach (var note in notes)
        {
            if (await SecurityManager.HasAccess(accessLevel, user, note))
            {
                result.Add(note);
            }
        }

        return result;
    }

    protected async Task<NoteNodeEntity?> GetValidParentAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        Guid? parentId
    )
    {
        if (!parentId.HasValue)
        {
            return null;
        }

        var parent = await GetNoteAsync(workspace, user, parentId.Value, AccessLevel.Write);
        if (parent.Type != NoteNodeType.Folder)
        {
            throw new DataValidationException("Parent note must be a folder");
        }

        return parent;
    }

    protected async Task EnsureFolderIsNotMovedIntoItselfOrDescendantAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity folder,
        NoteNodeEntity parent
    )
    {
        if (folder.Id == parent.Id)
        {
            throw new DataValidationException("Folder can not be moved into itself");
        }

        var allNodes = await NoteDao.GetWorkspaceNodesAsync(workspace);
        var descendantIds = GetNodeWithDescendants(folder, allNodes)
            .Select(item => item.Id)
            .ToHashSet();
        if (descendantIds.Contains(parent.Id))
        {
            throw new DataValidationException("Folder can not be moved into its descendant");
        }
    }

    protected async Task EnsureLinkedEntityExistsAsync(
        WorkspaceEntity workspace,
        NoteLinkEntityType entityType,
        Guid entityId
    )
    {
        switch (entityType)
        {
            case NoteLinkEntityType.Client:
                var client = await ClientDao.GetById(entityId, workspace);
                RecordNotFoundException.ThrowIfNull(client);
                break;
            case NoteLinkEntityType.Project:
                var project = await ProjectDao.GetById(entityId);
                if (project == null || project.Client.Workspace.Id != workspace.Id)
                {
                    throw new RecordNotFoundException();
                }
                break;
            case NoteLinkEntityType.Task:
                var task = await TaskDao.GetById(entityId);
                if (task == null || task.Workspace.Id != workspace.Id)
                {
                    throw new RecordNotFoundException();
                }
                break;
            default:
                throw new DataValidationException("Unsupported linked entity type");
        }
    }

    protected async Task<int> ResolveSortOrderAsync(
        WorkspaceEntity workspace,
        NoteNodeEntity? parent,
        int? sortOrder
    )
    {
        return sortOrder ?? await NoteDao.GetNextSortOrderAsync(workspace, parent);
    }

    protected static void EnsureDocument(NoteNodeEntity note)
    {
        if (note.Type != NoteNodeType.Document)
        {
            throw new DataValidationException("Note must be a document");
        }
    }

    protected static string NormalizeTitle(string title)
    {
        var normalizedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new DataValidationException("Title is required");
        }
        if (normalizedTitle.Length > TitleMaxLength)
        {
            throw new DataValidationException("Title is too long");
        }

        return normalizedTitle;
    }

    protected static string NormalizeMarkdown(string? markdownContent)
    {
        var normalizedMarkdown = markdownContent ?? string.Empty;
        if (normalizedMarkdown.Length > MarkdownMaxLength)
        {
            throw new DataValidationException("Markdown content is too long");
        }

        return normalizedMarkdown;
    }

    protected static void EnsureNoDuplicateLinks(IEnumerable<NoteLinkRequestDto> links)
    {
        var duplicateLink = links
            .GroupBy(item => new { item.EntityType, item.EntityId })
            .Any(item => item.Count() > 1);
        if (duplicateLink)
        {
            throw new RecordIsExistsException("Note link already exists");
        }
    }

    protected static void SetUpdatedBy(NoteNodeEntity node, UserEntity user, DateTime? now = null)
    {
        node.UpdatedByUser = user;
        node.UpdatedAt = now ?? DateTime.UtcNow;
    }

    protected static ICollection<NoteNodeEntity> GetNodeWithDescendants(
        NoteNodeEntity node,
        ICollection<NoteNodeEntity> allNodes
    )
    {
        var result = new List<NoteNodeEntity> { node };
        var pendingIds = new Queue<Guid>();
        pendingIds.Enqueue(node.Id);

        while (pendingIds.TryDequeue(out var parentId))
        {
            var children = allNodes.Where(item => item.Parent?.Id == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child);
                pendingIds.Enqueue(child.Id);
            }
        }

        return result;
    }
}
