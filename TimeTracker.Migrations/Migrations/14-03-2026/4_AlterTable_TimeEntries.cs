using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(4)]
    public class _4_AlterTable_TimeEntries : MyMigration
    {
        public override void Up()
        {
            Execute.Sql("TRUNCATE TABLE time_entries;");
            
            Alter.Table("time_entries")
                .AddColumn("date").AsCustom("date").NotNullable()
                .AlterColumn("start_time").AsCustom("time").NotNullable()
                .AlterColumn("end_time").AsCustom("time").Nullable();
            
            base.Up();
        }
    }
}
