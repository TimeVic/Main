using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(31)]
    public class _31_AlterTable_TaskHistory : MyMigration
    {
        public override void Up()
        {
            Alter.Table("task_history_items")
                .AddColumn("is_new_task").AsBoolean().NotNullable().WithDefaultValue(false);

            base.Up();
        }
    }
}
