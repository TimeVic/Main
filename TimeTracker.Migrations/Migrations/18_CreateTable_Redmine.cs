using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(18)]
    public class _18_CreateTable_Redmine : MyMigration
    {
        public override void Up()
        {
            Create.Table("workspace_setting_redmines")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("url").AsString(200).NotNullable()
                .WithColumn("api_key").AsString(100).NotNullable()
                .WithColumn("activity_id").AsInt64().NotNullable()
                .WithColumn("redmine_user_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Alter.Table("time_entries")
                .AddColumn("redmine_id").AsString(100).Nullable();

            Create.UniqueConstraint()
                .OnTable("workspace_setting_redmines").Columns("workspace_id", "user_id");
            
            Create.ForeignKey()
                .FromTable("workspace_setting_redmines").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("workspace_setting_redmines").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
