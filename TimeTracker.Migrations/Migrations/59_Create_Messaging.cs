using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(59)]
    public class _59_Create_Messaging : MyMigration
    {
        public override void Up()
        {
            Alter.Table("channel_members").InSchema("messaging")
                .AddColumn("deactivated_at").AsDateTime().Nullable();
            
            base.Up();
        }
    }
}
