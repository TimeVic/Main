using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(6)]
    public class _6_CreateTable_Payments : MyMigration
    {
        public override void Up()
        {
            Create.Table("payments")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("project_id").AsInt64().Nullable()
                .WithColumn("client_id").AsInt64().NotNullable()
                .WithColumn("payment_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("amount").AsDecimal(8, 2).NotNullable()
                .WithColumn("description").AsString(512).Nullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
