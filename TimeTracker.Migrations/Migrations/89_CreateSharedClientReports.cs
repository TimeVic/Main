using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(89)]
public class _89_CreateSharedClientReports : MyMigration
{
    public override void Up()
    {
        Create.Table("shared_client_reports")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable()
            .WithColumn("client_id").AsGuid().NotNullable().Unique()
            .WithColumn("token").AsString(64).NotNullable().Unique()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_show_tasks").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsCustom("timestamp").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp").Nullable();

        Create.ForeignKey()
            .FromTable("shared_client_reports").ForeignColumn("client_id")
            .ToTable("clients").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        base.Up();
    }
}
