using Persistence.Transactions.Behaviors;

namespace TimeTracker.Business.Testing.Services;

public class DbCleanUpService: IDbCleanUpService
{
    private readonly IDbSessionProvider _sessionProvider;

    public DbCleanUpService(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task CleanUp()
    {
        var tables = new[]
        {
            "messaging.activities",
            "messaging.counters",
            "messaging.messages",
            "messaging.connections",
            "messaging.channel_members",
            "messaging.channels",
            
            "notifications",
            "note_links",
            "note_node_history",
            "note_contents",
            "note_node_stored_files",
            "note_nodes",
            "client_payments",
            "member_payments",
            "queues",
            "task_comment_stored_files",
            "task_comment_watchers",
            "task_history_items",
            "task_stored_files",
            "user_stored_files",
            "task_tags",
            "time_entry_tags",
            "user_magic_tokens",
            "user_jwt_tokens",
            "user_access_tokens",
            "user_notification_tokens",
            "user_reset_password_requests",
            "workspace_member_project_accesses",
            "workspace_members",
            "workspace_setting_clickups",
            "workspace_setting_redmines",
            
            "fs_access_keys",
            "fs_files",
            "fs_directories",
            "fs_buckets",
            
            "goals_tracker_completion_markers",
            "goals_tracker_items",
            "goals_tracker_notes",
            "goals_trackers",
            
            "workspace_setting_jiras",
            "stored_files",
            "tags",
            "task_comments",
            "time_entries",
            "tasks",
            "task_lists",
            "projects",
            "clients",
            "workspaces",
            "users",
            "sequences",
        };

        // CASCADE removes dependent records before parent records and avoids foreign key violations during cleanup.
        await _sessionProvider.CurrentSession
            .CreateSQLQuery($"truncate table {string.Join(", ", tables)} cascade;")
            .ExecuteUpdateAsync();

        await _sessionProvider.CurrentSession.FlushAsync();
        _sessionProvider.CloseCurrentSession();
    }
}
