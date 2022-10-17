using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(11)]
    public class _11_Alter_Payments : MyMigration
    {
        public override void Up()
        {
            Alter.Table("payments")
                .AlterColumn("user_id").AsInt64().NotNullable()
                .AlterColumn("workspace_id").AsInt64().Nullable();
            
            base.Up();
        }
    }
}
