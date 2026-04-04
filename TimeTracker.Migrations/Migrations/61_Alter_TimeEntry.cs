using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(61)]
    public class _61_Alter_TimeEntry : MyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                ALTER TABLE time_entries
                    RENAME COLUMN start_time TO old_start_time;

                ALTER TABLE time_entries
                    RENAME COLUMN end_time TO old_end_time;

            ");

            Alter.Table("time_entries")
                .AddColumn("start_time").AsCustom("timestamptz").Nullable()
                .AddColumn("end_time").AsCustom("timestamptz").Nullable();
            
            Execute.Sql(@"
                UPDATE time_entries
                SET 
                    start_time = date + old_start_time,
                    end_time   = date + old_end_time;

            ");
            
            Alter.Table("time_entries")
                .AlterColumn("start_time").AsCustom("timestamptz").NotNullable()
                .AlterColumn("end_time").AsCustom("timestamptz").Nullable()
                
                .AlterColumn("date").AsCustom("date").Nullable()
                .AlterColumn("old_start_time").AsCustom("time").Nullable()
                .AlterColumn("old_end_time").AsCustom("time").Nullable();
            
            base.Up();
        }
    }
}
