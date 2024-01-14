using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(46)]
    public class _46_Alter_ChangeDateTimeType : MyMigration
    {
        public override void Up()
        {
            Alter.Table("goals_trackers")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("goals_tracker_items")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("goals_tracker_completion_markers")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("goals_tracker_notes")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("tasks")
                .AlterColumn("reminder_time").AsDateTime2().Nullable()
                .AlterColumn("reminded_time").AsDateTime2().Nullable()
                .AlterColumn("start_time").AsDateTime2().Nullable()
                .AlterColumn("end_time").AsDateTime2().Nullable()
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("create_time").AsDateTime2().NotNullable();
            
            Alter.Table("tags")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("task_comments")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("task_history_items")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("start_time").AsDateTime2().Nullable()
                .AlterColumn("end_time").AsDateTime2().Nullable();
            
            Alter.Table("task_lists")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("user_access_tokens")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("expiration_time").AsDateTime2().NotNullable();
            
            Alter.Table("user_jwt_tokens")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("expiration_time").AsDateTime2().NotNullable();
            
            Alter.Table("user_notification_tokens")
                .AlterColumn("create_time").AsDateTime2().NotNullable();
            
            Alter.Table("user_reset_password_requests")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("expiration_time").AsDateTime2().NotNullable();
            
            Alter.Table("workspace_setting_jiras")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("workspace_setting_redmines")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            base.Up();
        }
    }
}
