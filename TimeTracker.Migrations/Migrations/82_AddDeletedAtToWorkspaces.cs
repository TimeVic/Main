using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(82)]
public class _82_AddDeletedAtToWorkspaces : MyMigration
{
    public override void Up()
    {
        Alter.Table("workspaces")
            .AddColumn("deleted_at").AsCustom("timestamp").Nullable();

        Create.Index()
            .OnTable("workspaces")
            .OnColumn("deleted_at");

        base.Up();
    }
}
