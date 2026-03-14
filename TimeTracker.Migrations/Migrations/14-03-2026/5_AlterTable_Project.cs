using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(5)]
    public class _5_AlterTable_Project : MyMigration
    {
        public override void Up()
        {
            Alter.Table("projects")
                .AddColumn("is_billable_by_default").AsBoolean().WithDefaultValue(true)
                .AddColumn("default_hourly_rate").AsDecimal(8, 2).Nullable()
                .AddColumn("is_archived").AsBoolean().WithDefaultValue(false);
            
            base.Up();
        }
    }
}
