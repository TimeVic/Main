using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(7)]
    public class _7_CreateTable_AppClickUp : MyMigration
    {
        public override void Up()
        {
            Create.Table("workspace_setting_clickups")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("security_key").AsString(512).Nullable()
                .WithColumn("team_id").AsString(512).Nullable()
                .WithColumn("is_custom_task_ids").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Alter.Table("time_entries")
                .AddColumn("user_id").AsInt64().Nullable()
                .AddColumn("task_id").AsString(512).Nullable()
                .AddColumn("clickup_id").AsInt64().Nullable();
            
            Create.ForeignKey()
                .FromTable("time_entries").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.UniqueConstraint()
                .OnTable("workspace_setting_clickups").Columns("workspace_id", "user_id");
            
            Create.ForeignKey()
                .FromTable("workspace_setting_clickups").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("workspace_setting_clickups").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
