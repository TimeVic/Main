using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(14)]
    public class _14_Alter_User_AddColumns : MyMigration
    {
        public override void Up()
        {
            Alter.Table("users")
                .AddColumn("timezone").AsString(100).NotNullable().WithDefaultValue("UTC");

            base.Up();
        }
    }
}
