using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(35)]
    public class _35_AlterTable_Task : MyMigration
    {
        public override void Up()
        {
            Delete.Column("notification_time").FromTable("tasks");
            Delete.Column("notification_time").FromTable("task_history_items");
            
            Alter.Table("tasks")
                .AddColumn("start_time").AsCustom("timestamptz").Nullable()
                .AddColumn("end_time").AsCustom("timestamptz").Nullable();
            
            Alter.Table("task_history_items")
                .AddColumn("start_time").AsCustom("timestamptz").Nullable()
                .AddColumn("end_time").AsCustom("timestamptz").Nullable();
            
            base.Up();
        }
    }
}
