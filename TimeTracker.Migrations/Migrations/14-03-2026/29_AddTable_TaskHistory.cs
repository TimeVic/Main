using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(29)]
    public class _29_AddTable_TaskHistory : MyMigration
    {
        public override void Up()
        {
            Create.Table("task_history_items")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("task_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                
                .WithColumn("title").AsString(1024).NotNullable()
                .WithColumn("tags").AsString(1024).Nullable()
                .WithColumn("attachments").AsString(10_000).Nullable()
                .WithColumn("description").AsString(10_000).Nullable()
                .WithColumn("notification_time").AsCustom("timestamptz").Nullable()
                .WithColumn("is_done").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("external_task_id").AsString(512).Nullable()
                .WithColumn("task_list_id").AsInt64().NotNullable()
                .WithColumn("assignee_user_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("task_history_items").ForeignColumn("task_id")
                .ToTable("tasks").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("task_history_items").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("task_history_items").ForeignColumn("task_list_id")
                .ToTable("task_lists").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("task_history_items").ForeignColumn("assignee_user_id")
                .ToTable("users").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
