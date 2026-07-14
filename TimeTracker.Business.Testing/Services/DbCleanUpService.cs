using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;

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
        await _sessionProvider.CurrentSession
            .CreateSQLQuery("update users set selected_workspace_id = null where selected_workspace_id is not null;")
            .ExecuteUpdateAsync();

        await _sessionProvider.CurrentSession
            .CreateSQLQuery("update note_nodes set last_content_id = null where last_content_id is not null;")
            .ExecuteUpdateAsync();

        var tables = new List<string>()
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

        foreach (var table in tables)
        {
            await _sessionProvider.CurrentSession.CreateSQLQuery($"delete from {table} where 1=1;").ExecuteUpdateAsync();    
        }
        _sessionProvider.CurrentSession.Clear();
    }
}
