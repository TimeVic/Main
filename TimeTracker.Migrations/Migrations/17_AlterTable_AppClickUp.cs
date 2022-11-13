using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(17)]
    public class _17_AlterTable_AppClickUp : MyMigration
    {
        public override void Up()
        {
            Alter.Table("workspace_setting_clickups")
                .AddColumn("is_fill_time_entry_with_task_details").AsBoolean().NotNullable().WithDefaultValue(true);

            base.Up();
        }
    }
}
