using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(48)]
    public class _48_Alter_GoalsTrackerItems : MyMigration
    {
        public override void Up()
        {
            Alter.Table("goals_tracker_items")
                .AddColumn("position").AsInt32().NotNullable().WithDefaultValue(0);
            
            base.Up();
        }
    }
}
