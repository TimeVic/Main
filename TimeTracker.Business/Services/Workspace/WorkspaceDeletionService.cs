using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage.Client;

namespace TimeTracker.Business.Services.Workspace;

public class WorkspaceDeletionService : IWorkspaceDeletionService
{
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IFileStorageGarageClient _storageClient;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public WorkspaceDeletionService(
        IDbSessionProvider dbSessionProvider,
        IFileStorageGarageClient storageClient,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _storageClient = storageClient;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task SoftDeleteAsync(WorkspaceEntity workspace)
    {
        if (workspace.IsDefault)
        {
            throw new DataValidationException("The default workspace cannot be deleted.");
        }

        if (workspace.DeletedAt != null)
        {
            return;
        }

        var session = _dbSessionProvider.CurrentSession;
        var users = await session.Query<UserEntity>()
            .Where(item => item.SelectedWorkspace != null && item.SelectedWorkspace.Id == workspace.Id)
            .ToListAsync();

        foreach (var user in users)
        {
            user.SelectedWorkspace = null;
            user.UpdatedAt = DateTime.UtcNow;
            await session.SaveAsync(user);
        }

        workspace.DeletedAt = DateTime.UtcNow;
        workspace.UpdatedAt = DateTime.UtcNow;
        await session.SaveAsync(workspace);
    }

    public async Task HardDeleteAsync(WorkspaceEntity workspace, CancellationToken cancellationToken = default)
    {
        var session = _dbSessionProvider.CurrentSession;
        var workspaceId = workspace.Id;
        var files = await GetWorkspaceFilesAsync(workspaceId, cancellationToken);

        await DeleteNotificationsAsync(workspaceId, cancellationToken);
        await DeleteMessagingAsync(workspaceId, cancellationToken);
        await DeleteNotesAsync(workspaceId, cancellationToken);
        await DeleteTimeEntriesAsync(workspaceId, cancellationToken);
        await DeleteTasksAsync(workspaceId, cancellationToken);
        await session.FlushAsync(cancellationToken);
        await DeleteGoalsAsync(workspaceId, cancellationToken);
        await DeletePaymentsAndProjectsAsync(workspaceId, cancellationToken);
        await DeleteWorkspaceConfigurationAsync(workspaceId, cancellationToken);
        await DeleteFilesAsync(files, cancellationToken);

        var selectedUsers = await session.Query<UserEntity>()
            .Where(item => item.SelectedWorkspace != null && item.SelectedWorkspace.Id == workspaceId)
            .ToListAsync(cancellationToken);
        foreach (var user in selectedUsers)
        {
            user.SelectedWorkspace = null;
            user.UpdatedAt = DateTime.UtcNow;
            await session.SaveAsync(user, cancellationToken);
        }

        await session.FlushAsync(cancellationToken);
        session.Clear();
        await DeleteWorkspaceMembersAsync(workspaceId, cancellationToken);
        await session.FlushAsync(cancellationToken);
        session.Clear();
        await session.Query<WorkspaceEntity>()
            .Where(item => item.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteNotificationsAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        await _dbSessionProvider.CurrentSession.Query<NotificationEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteMessagingAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var channelIds = await session.Query<MessagingChannelEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        await session.Query<MessagingActivityEntity>()
            .Where(item => channelIds.Contains(item.Channel.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<MessagingCounterEntity>()
            .Where(item => channelIds.Contains(item.Channel.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<MessagingMessageEntity>()
            .Where(item => channelIds.Contains(item.Channel.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<MessagingChannelMemberEntity>()
            .Where(item => channelIds.Contains(item.Channel.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<MessagingChannelEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteNotesAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var nodes = await session.Query<NoteNodeEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .ToListAsync(cancellationToken);
        var nodeIds = nodes.Select(item => item.Id).ToArray();

        foreach (var node in nodes)
        {
            node.Attachments.Clear();
            node.LastContent = null;
            await session.SaveAsync(node, cancellationToken);
        }

        await session.Query<NoteLinkEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
        await session.Query<NoteNodeHistoryEntity>()
            .Where(item => nodeIds.Contains(item.NoteNode.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<NoteContentEntity>()
            .Where(item => nodeIds.Contains(item.NoteNode.Id))
            .DeleteAsync(cancellationToken);

        var nodesById = nodes.ToDictionary(item => item.Id);
        foreach (var node in nodes.OrderByDescending(item => GetNoteDepth(item, nodesById)))
        {
            await session.DeleteAsync(node, cancellationToken);
        }
    }

    private static int GetNoteDepth(NoteNodeEntity node, IReadOnlyDictionary<Guid, NoteNodeEntity> nodesById)
    {
        var depth = 0;
        var parent = node.Parent;
        while (parent != null && nodesById.TryGetValue(parent.Id, out var parentNode))
        {
            depth++;
            parent = parentNode.Parent;
        }

        return depth;
    }

    private async Task DeleteTimeEntriesAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var entries = await session.Query<TimeEntryEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            entry.Tags.Clear();
            await session.DeleteAsync(entry, cancellationToken);
        }
    }

    private async Task DeleteTasksAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var taskListIds = await session.Query<TaskListEntity>()
            .Where(item => item.Project.Client.Workspace.Id == workspaceId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var tasks = await session.Query<TaskEntity>()
            .Where(item => taskListIds.Contains(item.TaskList.Id))
            .ToListAsync(cancellationToken);
        var taskIds = tasks.Select(item => item.Id).ToArray();

        var comments = await session.Query<TaskCommentEntity>()
            .Where(item => taskIds.Contains(item.Task.Id))
            .ToListAsync(cancellationToken);
        foreach (var comment in comments)
        {
            comment.Watchers.Clear();
            comment.Attachments.Clear();
            await session.DeleteAsync(comment, cancellationToken);
        }

        await session.Query<TaskHistoryItemEntity>()
            .Where(item => taskIds.Contains(item.Task.Id))
            .DeleteAsync(cancellationToken);

        foreach (var task in tasks)
        {
            task.Tags.Clear();
            task.Attachments.Clear();
            await session.DeleteAsync(task, cancellationToken);
        }

        await session.Query<TaskListEntity>()
            .Where(item => taskListIds.Contains(item.Id))
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteGoalsAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var trackerIds = await session.Query<GoalsTrackerEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var itemIds = await session.Query<GoalsTrackerItemEntity>()
            .Where(item => trackerIds.Contains(item.Tracker.Id))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        await session.Query<GoalsTrackerCompletionMarkerEntity>()
            .Where(item => itemIds.Contains(item.GoalsTrackerItem.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<GoalsTrackerItemEntity>()
            .Where(item => trackerIds.Contains(item.Tracker.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<GoalsTrackerNoteEntity>()
            .Where(item => trackerIds.Contains(item.Tracker.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<GoalsTrackerEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeletePaymentsAndProjectsAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        var clientIds = await session.Query<ClientEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var projectIds = await session.Query<ProjectEntity>()
            .Where(item => clientIds.Contains(item.Client.Id))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        await session.Query<MemberPaymentEntity>()
            .Where(item => projectIds.Contains(item.Project.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<ClientPaymentEntity>()
            .Where(item => clientIds.Contains(item.Client.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<WorkspaceMemberProjectAccessEntity>()
            .Where(item => projectIds.Contains(item.Project.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<ProjectEntity>()
            .Where(item => clientIds.Contains(item.Client.Id))
            .DeleteAsync(cancellationToken);
        await session.Query<ClientEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteWorkspaceConfigurationAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var session = _dbSessionProvider.CurrentSession;
        await session.Query<WorkspaceSettingsClickUpEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
        await session.Query<WorkspaceSettingsJiraEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
        await session.Query<WorkspaceSettingsRedmineEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
        await session.Query<TagEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .DeleteAsync(cancellationToken);
    }

    private async Task DeleteWorkspaceMembersAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var membershipIds = await _dbSessionProvider.CurrentSession.Query<WorkspaceMemberEntity>()
            .Where(item => item.Workspace.Id == workspaceId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        foreach (var membershipId in membershipIds)
        {
            await _workspaceAccessService.RemoveAccessAsync(membershipId);
        }
    }

    private async Task<IReadOnlyCollection<StoredFileEntity>> GetWorkspaceFilesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken
    )
    {
        var files = await _dbSessionProvider.CurrentSession.Query<StoredFileEntity>()
            .Where(item => item.Tasks.Any(task => task.TaskList.Project.Client.Workspace.Id == workspaceId)
                || item.TaskComments.Any(comment => comment.Task.TaskList.Project.Client.Workspace.Id == workspaceId)
                || item.NoteNodes.Any(note => note.Workspace.Id == workspaceId))
            .ToListAsync(cancellationToken);

        return files.Where(item => IsWorkspaceOwnedFile(item, workspaceId)).ToList();
    }

    private static bool IsWorkspaceOwnedFile(StoredFileEntity file, Guid workspaceId)
    {
        return file.Users.Count == 0
            && file.Tasks.All(item => item.TaskList.Project.Client.Workspace.Id == workspaceId)
            && file.TaskComments.All(item => item.Task.TaskList.Project.Client.Workspace.Id == workspaceId)
            && file.NoteNodes.All(item => item.Workspace.Id == workspaceId);
    }

    private async Task DeleteFilesAsync(
        IReadOnlyCollection<StoredFileEntity> files,
        CancellationToken cancellationToken
    )
    {
        var session = _dbSessionProvider.CurrentSession;
        foreach (var file in files)
        {
            await _storageClient.Delete(file.CloudFilePath, cancellationToken);
            if (!string.IsNullOrWhiteSpace(file.ThumbCloudFilePath))
            {
                await _storageClient.Delete(file.ThumbCloudFilePath, cancellationToken);
            }

            file.Tasks.Clear();
            file.TaskComments.Clear();
            file.NoteNodes.Clear();
            await session.DeleteAsync(file, cancellationToken);
        }
    }
}
