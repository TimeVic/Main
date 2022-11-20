using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(19)]
    public class _19_Insert_QueueChannels : MyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("queue_channels")
                .Row(new { id = 3, name = "ExternalClient" });

            Alter.Table("time_entries")
                .AlterColumn("clickup_id").AsString(100).Nullable();
            base.Up();
        }
    }
}
