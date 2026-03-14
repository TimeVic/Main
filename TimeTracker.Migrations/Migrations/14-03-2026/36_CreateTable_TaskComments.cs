using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(36)]
    public class _36_CreateTable_TaskComments : MyMigration
    {
        public override void Up()
        {
            Create.Table("task_comments")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("task_id").AsInt64().NotNullable().ForeignKey("tasks", "id")
                .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("comment").AsString(10000).NotNullable()
                .WithColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("task_comment_watchers")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("comment_id").AsInt64().NotNullable().ForeignKey("task_comments", "id")
                .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id");
            
            Create.Table("task_comment_stored_files")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("comment_id").AsInt64().NotNullable()
                .ForeignKey("task_comments", "id")
                .WithColumn("stored_file_id").AsInt64().NotNullable()
                .ForeignKey("stored_files", "id");
            
            base.Up();
        }
    }
}
