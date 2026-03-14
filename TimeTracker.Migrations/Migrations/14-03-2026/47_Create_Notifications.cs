using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(47)]
    public class _47_Create_Notifications : MyMigration
    {
        public override void Up()
        {
            Create.Table("notification_types")
                .WithColumn("id").AsInt16().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();
            
            Insert.IntoTable("notification_types")
                .Row(new {id = 1, name = "AddEntity"})
                .Row(new {id = 2, name = "EditEntity"})
                .Row(new {id = 3, name = "DeleteEntity"})
                .Row(new {id = 4, name = "Reminder"});
            
            Create.Table("notifications")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("type").AsInt64().NotNullable().ForeignKey("notification_types", "id")
                .WithColumn("workspace_id").AsInt64().NotNullable().ForeignKey("workspaces", "id")
                .WithColumn("performed_user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("receiver_user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("is_read").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("comment").AsString(2056).Nullable()
                
                .WithColumn("task_id").AsInt64().Nullable().ForeignKey("tasks", "id")
                .WithColumn("task_comment_id").AsInt64().Nullable().ForeignKey("task_comments", "id")
                
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
