using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(3)]
    public class _3_Alter_Users : MyMigration
    {
        public override void Up()
        {
            Delete.Index().OnTable("users").OnColumn("user_name");
            base.Up();
        }
    }
}
