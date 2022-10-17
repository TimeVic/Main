using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(12)]
    public class _12_Alter_TimeEntries : MyMigration
    {
        public override void Up()
        {
            Alter.Table("time_entries")
                .AlterColumn("user_id").AsInt64().NotNullable();
            
            base.Up();
        }
    }
}
