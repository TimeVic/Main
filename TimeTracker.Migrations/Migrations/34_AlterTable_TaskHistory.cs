using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(34)]
    public class _34_AlterTable_TaskHistory : MyMigration
    {
        public override void Up()
        {
            Alter.Table("task_history_items")
                .AddColumn("priority").AsInt32().NotNullable().WithDefaultValue("1")
                .ForeignKey("task_priorities", "id");
            
            base.Up();
        }
    }
}
