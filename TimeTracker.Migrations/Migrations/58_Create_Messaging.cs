using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(58)]
    public class _58_Create_Messaging : MyMigration
    {
        public override void Up()
        {
            Alter.Table("channels").InSchema("messaging")
                .AddColumn("slug").AsString().NotNullable();
            
            Delete.Column("name").FromTable("channels").InSchema("messaging");
            
            Create.UniqueConstraint().OnTable("channels").WithSchema("messaging").Columns("workspace_id", "slug");
            
            base.Up();
        }
    }
}
