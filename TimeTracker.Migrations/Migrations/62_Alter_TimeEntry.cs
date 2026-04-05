using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(62)]
    public class _62_Alter_TimeEntry : MyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                ALTER TABLE time_entries
                ALTER COLUMN start_time
                TYPE timestamp
                USING start_time AT TIME ZONE 'UTC';
            ");
            Execute.Sql(@"
                ALTER TABLE time_entries
                ALTER COLUMN end_time
                TYPE timestamp
                USING end_time AT TIME ZONE 'UTC';
            ");
            base.Up();
        }
    }
}
