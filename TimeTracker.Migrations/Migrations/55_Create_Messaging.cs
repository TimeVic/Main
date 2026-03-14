using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(55)]
    public class _55_Create_Messaging : MyMigration
    {
        public override void Up()
        {
            Create.Schema("messaging");
            
            Create.Table("channels").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("workspace_id").AsGuid().NotNullable()
                .WithColumn("type").AsString(256).NotNullable()
                .WithColumn("name").AsString(256).NotNullable()
                .WithColumn("created_by_id").AsGuid().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.Table("messaging_channel_type").InSchema("enum")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(200).Unique();

            Insert.IntoTable("messaging_channel_type").InSchema("enum")
                .Row(new { id = 1, name = "Common" })
                .Row(new { id = 2, name = "Direct" });
            
            Create.Table("messages").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("channel_id").AsGuid().NotNullable().WithDefaultValue(1)
                .WithColumn("text").AsString(110000).NotNullable()
                .WithColumn("created_by_id").AsGuid().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.Table("channel_members").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("channel_id").AsGuid().NotNullable()
                .WithColumn("member_id").AsGuid().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.ForeignKey().FromTable("channels")
                .InSchema("messaging")
                .ForeignColumn("type")
                .ToTable("messaging_channel_type")
                .InSchema("enum")
                .PrimaryColumn("id");
            
            Create.ForeignKey().FromTable("messages")
                .InSchema("messaging")
                .ForeignColumn("channel_id")
                .ToTable("channels")
                .InSchema("messaging")
                .PrimaryColumn("id");
            
            Create.ForeignKey().FromTable("messages")
                .InSchema("messaging")
                .ForeignColumn("created_by_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            Create.ForeignKey().FromTable("channels")
                .InSchema("messaging")
                .ForeignColumn("created_by_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            base.Up();
        }
    }
}
