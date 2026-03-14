using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(20)]
    public class _20_Alter_TimeEntry : MyMigration
    {
        public override void Up()
        {
            Alter.Table("time_entries")
                .AddColumn("is_marked_to_delete").AsBoolean().NotNullable().WithDefaultValue(false);
            base.Up();
        }
    }
}
