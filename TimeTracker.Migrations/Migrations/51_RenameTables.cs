using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(51)]
    public class _51_RenameTables : MyMigration
    {
        public override void Up()
        {
            Rename.Column("create_time").OnTable("time_entries").To("created_at");
            Rename.Column("update_time").OnTable("time_entries").To("updated_at");
            
            Rename.Column("create_time").OnTable("tags").To("created_at");
            Rename.Column("update_time").OnTable("tags").To("updated_at");
            
            Rename.Column("create_time").OnTable("stored_files").To("created_at");
            
            Rename.Column("create_time").OnTable("queues").To("created_at");
            Rename.Column("update_time").OnTable("queues").To("updated_at");
            
            Rename.Column("create_time").OnTable("projects").To("created_at");
            Rename.Column("update_time").OnTable("projects").To("updated_at");
            
            Rename.Column("create_time").OnTable("payments").To("created_at");
            Rename.Column("update_time").OnTable("payments").To("updated_at");
            
            Rename.Column("create_time").OnTable("clients").To("created_at");
            Rename.Column("update_time").OnTable("clients").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspaces").To("created_at");
            Rename.Column("update_time").OnTable("workspaces").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspace_setting_clickups").To("created_at");
            Rename.Column("update_time").OnTable("workspace_setting_clickups").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspace_setting_jiras").To("created_at");
            Rename.Column("update_time").OnTable("workspace_setting_jiras").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspace_setting_redmines").To("created_at");
            Rename.Column("update_time").OnTable("workspace_setting_redmines").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspace_memberships").To("created_at");
            Rename.Column("update_time").OnTable("workspace_memberships").To("updated_at");
            
            Rename.Column("create_time").OnTable("workspace_membership_project_accesses").To("created_at");
            Rename.Column("update_time").OnTable("workspace_membership_project_accesses").To("updated_at");
            
            Rename.Column("create_time").OnTable("users").To("created_at");
            Rename.Column("update_time").OnTable("users").To("updated_at");
            
            Rename.Column("create_time").OnTable("user_access_tokens").To("created_at");
            
            Rename.Column("create_time").OnTable("user_jwt_tokens").To("created_at");
            
            Rename.Column("create_time").OnTable("user_notification_tokens").To("created_at");
            
            Rename.Column("create_time").OnTable("user_reset_password_requests").To("created_at");
            
            Rename.Column("create_time").OnTable("tasks").To("created_at");
            Rename.Column("update_time").OnTable("tasks").To("updated_at");
            
            Rename.Column("create_time").OnTable("task_comments").To("created_at");
            Rename.Column("update_time").OnTable("task_comments").To("updated_at");
            
            Rename.Column("create_time").OnTable("task_history_items").To("created_at");
            
            Rename.Column("create_time").OnTable("task_lists").To("created_at");
            Rename.Column("update_time").OnTable("task_lists").To("updated_at");
            
            Rename.Column("create_time").OnTable("notifications").To("created_at");
            Rename.Column("update_time").OnTable("notifications").To("updated_at");
            Alter.Table("notifications").AlterColumn("updated_at").AsDateTime().Nullable();
            
            Rename.Column("create_time").OnTable("goals_trackers").To("created_at");
            Rename.Column("update_time").OnTable("goals_trackers").To("updated_at");
            
            Rename.Column("create_time").OnTable("goals_tracker_completion_markers").To("created_at");
            Rename.Column("update_time").OnTable("goals_tracker_completion_markers").To("updated_at");
            
            Rename.Column("create_time").OnTable("goals_tracker_items").To("created_at");
            Rename.Column("update_time").OnTable("goals_tracker_items").To("updated_at");
            
            Rename.Column("create_time").OnTable("goals_tracker_notes").To("created_at");
            Rename.Column("update_time").OnTable("goals_tracker_notes").To("updated_at");
            
            Rename.Column("create_time").OnTable("fs_access_keys").To("created_at");
            Rename.Column("update_time").OnTable("fs_access_keys").To("updated_at");
            
            base.Up();
        }
    }
}
