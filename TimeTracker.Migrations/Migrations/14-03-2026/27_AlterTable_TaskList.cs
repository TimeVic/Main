using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(27)]
    public class _27_AlterTable_TaskList : MyMigration
    {
        public override void Up()
        {
            Alter.Table("task_lists")
                .AddColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false);
            
            base.Up();
        }
    }
}
