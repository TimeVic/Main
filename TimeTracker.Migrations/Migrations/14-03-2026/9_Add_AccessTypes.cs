using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(9)]
    public class _9_Add_AccessTypes : MyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("membership_access_types")
                .Row(new {id = 3, name = "Owner"});

            base.Up();
        }
    }
}
