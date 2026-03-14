using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(2)]
    public class _2_CreateTable_Queue : MyMigration
    {
        public override void Up()
        {
            Create.Table("queue_statuses")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(200).Unique();

            Insert.IntoTable("queue_statuses")
                .Row(new { id = 1, name = "Pending" })
                .Row(new { id = 2, name = "InProcess" })
                .Row(new { id = 3, name = "Success" })
                .Row(new { id = 4, name = "Fail" });
            
            Create.Table("queue_channels")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(200).Unique();

            Insert.IntoTable("queue_channels")
                .Row(new { id = 1, name = "Default" })
                .Row(new { id = 2, name = "Notifications" });
            
            Create.Table("queues")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("status").AsInt64().NotNullable()
                .WithColumn("error").AsString(1000).Nullable()
                .WithColumn("channel").AsInt64().NotNullable()
                .WithColumn("context_type").AsString(512).Nullable()
                .WithColumn("context_data").AsString().Nullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("queues").ForeignColumn("status")
                .ToTable("queue_statuses").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("queues").ForeignColumn("channel")
                .ToTable("queue_channels").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
