using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(50)]
    public class _50_Alter_Queue : MyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"create schema enum;");
            
            Create.Table("queue_priorities").InSchema("enum")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(200).Unique();

            Insert.IntoTable("queue_priorities").InSchema("enum")
                .Row(new { id = 1, name = "Lowest" })
                .Row(new { id = 2, name = "Low" })
                .Row(new { id = 3, name = "Normal" })
                .Row(new { id = 4, name = "High" })
                .Row(new { id = 5, name = "Highest" });
            
            Alter.Table("queues")
                .AddColumn("priority").AsInt64().WithDefaultValue(3)
                .AddColumn("process_at").AsCustom("timestamp").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.ForeignKey().FromTable("queues").ForeignColumn("priority").ToTable("queue_priorities")
                .InSchema("enum").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
