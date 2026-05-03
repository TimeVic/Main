using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(69)]
public class _69_CreateTable_ClientPayments : MyMigration
{
    public override void Up()
    {
        Create.Table("client_payments")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("workspace_id").AsGuid().NotNullable()
            .WithColumn("client_id").AsGuid().NotNullable()
            .WithColumn("project_id").AsGuid().Nullable()
            .WithColumn("payment_time").AsCustom("timestamptz").NotNullable()
            .WithColumn("amount").AsDecimal(8, 2).NotNullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("created_at").AsCustom("timestamptz").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamptz").Nullable();

        Create.ForeignKey()
            .FromTable("client_payments").ForeignColumn("workspace_id")
            .ToTable("workspaces").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("client_payments").ForeignColumn("client_id")
            .ToTable("clients").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("client_payments").ForeignColumn("project_id")
            .ToTable("projects").PrimaryColumn("id");

        base.Up();
    }
}
