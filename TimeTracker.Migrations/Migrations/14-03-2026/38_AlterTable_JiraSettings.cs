using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(38)]
    public class _38_AlterTable_JiraSettings : MyMigration
    {
        public override void Up()
        {
            Alter.Table("workspace_setting_jiras")
                .AddColumn("url").AsString(256).Nullable();
            
            base.Up();
        }
    }
}
