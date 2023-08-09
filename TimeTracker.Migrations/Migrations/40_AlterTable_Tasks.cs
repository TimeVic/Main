using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(40)]
    public class _40_AlterTable_Tasks : MyMigration
    {
        public override void Up()
        {
            Alter.Table("tasks")
                .AddColumn("task_id").AsInt64().NotNullable().WithDefaultValue(0);
            
            Execute.Sql("UPDATE tasks SET task_id = id WHERE 1=1");

            Create.UniqueConstraint().OnTable("tasks").Columns("task_list_id", "task_id");
            
            base.Up();
        }
    }
}
