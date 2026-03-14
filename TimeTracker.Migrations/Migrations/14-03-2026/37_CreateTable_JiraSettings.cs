using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(37)]
    public class _37_CreateTable_JiraSettings : MyMigration
    {
        public override void Up()
        {
            Alter.Table("time_entries")
                .AddColumn("jira_id").AsInt64().Nullable();
            
            Create.Table("workspace_setting_jiras")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("api_key").AsString(1024).Nullable()
                .WithColumn("user_name").AsString(256).Nullable()
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_fill_time_entry_with_task_details").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
