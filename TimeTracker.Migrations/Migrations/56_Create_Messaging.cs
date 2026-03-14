using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(56)]
    public class _56_Create_Messaging : MyMigration
    {
        public override void Up()
        {
            Create.Table("connections").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("connection_id").AsString(256).NotNullable()
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.ForeignKey().FromTable("connections")
                .InSchema("messaging")
                .ForeignColumn("user_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            Create.Index().OnTable("connections").InSchema("messaging").OnColumn("connection_id");
            Create.Index().OnTable("connections").InSchema("messaging").OnColumn("user_id");
            
            base.Up();
        }
    }
}
