using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(24)]
    public class _24_CreateTable_Task : MyMigration
    {
        public override void Up()
        {
            Create.Table("task_lists")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("name").AsString(1024).NotNullable()
                .WithColumn("project_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();

            Create.ForeignKey()
                .FromTable("task_lists").ForeignColumn("project_id")
                .ToTable("projects").PrimaryColumn("id");
            
            Create.Table("tasks")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("title").AsString(1024).NotNullable()
                .WithColumn("description").AsString(10_000).Nullable()
                .WithColumn("notification_time").AsCustom("timestamptz").Nullable()
                .WithColumn("is_done").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("task_list_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("tasks").ForeignColumn("task_list_id")
                .ToTable("task_lists").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("tasks").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");

            Create.Table("task_stored_files")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("task_id").AsInt64().NotNullable()
                .WithColumn("stored_file_id").AsInt64().NotNullable();
            
            Create.ForeignKey()
                .FromTable("task_stored_files").ForeignColumn("task_id")
                .ToTable("tasks").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("task_stored_files").ForeignColumn("stored_file_id")
                .ToTable("stored_files").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
