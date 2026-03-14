using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(3)]
    public class _3_AlterTable_Workspaces : MyMigration
    {
        public override void Up()
        {
            Alter.Table("workspaces")
                .AddColumn("is_default").AsBoolean().NotNullable().WithDefaultValue(false);
            
            base.Up();
        }
    }
}
