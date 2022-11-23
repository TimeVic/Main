using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(21)]
    public class _21_Alter_WorkspaceSettings : MyMigration
    {
        public override void Up()
        {
            Alter.Table("workspace_setting_redmines")
                .AddColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false);
            
            Alter.Table("workspace_setting_clickups")
                .AddColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false);
            base.Up();
        }
    }
}
