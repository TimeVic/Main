using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(57)]
    public class _57_Create_Messaging : MyMigration
    {
        public override void Up()
        {
            Create.Table("counters").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("counter").AsInt64().NotNullable()
                .WithColumn("channel_id").AsGuid().NotNullable()
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.ForeignKey().FromTable("counters")
                .InSchema("messaging")
                .ForeignColumn("channel_id")
                .ToTable("channels")
                .InSchema("messaging")
                .PrimaryColumn("id");
            
            Create.ForeignKey().FromTable("counters")
                .InSchema("messaging")
                .ForeignColumn("user_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            Create.Index().OnTable("counters").InSchema("messaging").OnColumn("channel_id");
            Create.Index().OnTable("counters").InSchema("messaging").OnColumn("user_id");
            
            Create.Table("activities").InSchema("messaging")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("channel_id").AsGuid().NotNullable()
                .WithColumn("is_writing").AsBoolean().NotNullable()
                .WithColumn("writing_started_at").AsDateTime().Nullable()
                .WithColumn("updated_at").AsDateTime().Nullable()
                .WithColumn("created_at").AsDateTime().NotNullable();
        
            Create.ForeignKey().FromTable("activities")
                .InSchema("messaging")
                .ForeignColumn("channel_id")
                .ToTable("channels")
                .InSchema("messaging")
                .PrimaryColumn("id");
            
            Create.ForeignKey().FromTable("activities")
                .InSchema("messaging")
                .ForeignColumn("user_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            Create.Index().OnTable("activities").InSchema("messaging").OnColumn("channel_id");
            Create.Index().OnTable("activities").InSchema("messaging").OnColumn("user_id");

            Alter.Table("channels").InSchema("messaging")
                .AddColumn("user_id").AsGuid().Nullable();
            
            Create.ForeignKey().FromTable("channels")
                .InSchema("messaging")
                .ForeignColumn("user_id")
                .ToTable("users")
                .PrimaryColumn("id");
            
            Create.Index().OnTable("channels").InSchema("messaging").OnColumn("user_id");
            
            base.Up();
        }
    }
}
