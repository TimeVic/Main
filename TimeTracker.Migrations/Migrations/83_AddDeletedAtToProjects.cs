using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(83)]
public class _83_AddDeletedAtToProjects : MyMigration
{
    public override void Up()
    {
        Alter.Table("projects")
            .AddColumn("deleted_at").AsCustom("timestamp").Nullable();

        Create.Index()
            .OnTable("projects")
            .OnColumn("deleted_at");

        base.Up();
    }
}
