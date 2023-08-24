using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(41)]
    public class _41_AlterTable_Tasks : MyMigration
    {
        public override void Up()
        {
            Alter.Table("tasks")
                .AddColumn("position_index").AsInt32().NotNullable().WithDefaultValue(0);
            
            base.Up();
        }
    }
}
