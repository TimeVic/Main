using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(30)]
    public class _30_AlterTable_TaskHistory : MyMigration
    {
        public override void Up()
        {
            Alter.Table("task_history_items")
                .AddColumn("is_notified").AsBoolean().NotNullable().WithDefaultValue(false);

            base.Up();
        }
    }
}
