using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(25)]
    public class _25_AlterTable_TimeEntry : MyMigration
    {
        public override void Up()
        {
            Alter.Table("time_entries")
                .AddColumn("internal_task_id").AsInt64().Nullable()
                .ForeignKey("tasks", "id");

            Alter.Table("tasks")
                .AddColumn("external_task_id").AsString(512).Nullable();
            
            base.Up();
        }
    }
}
