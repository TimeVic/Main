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
        var tables = new List<string>()
        {
            "notifications",
            "payments",
            "queues",
            "task_comment_stored_files",
            "task_comment_watchers",
            "task_history_items",
            "task_stored_files",
            "task_stored_files",
            "task_tags",
            "time_entry_tags",
            "user_jwt_tokens",
            "user_access_tokens",
            "user_notification_tokens",
            "user_reset_password_requests",
            "workspace_membership_project_accesses",
            "workspace_memberships",
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
        };

        foreach (var table in tables)
        {
            await _sessionProvider.CurrentSession.CreateSQLQuery($"delete from {table} where 1=1;").ExecuteUpdateAsync();    
        }

        await _sessionProvider.PerformCommitAsync();
        _sessionProvider.CurrentSession.Clear();
    }
}
