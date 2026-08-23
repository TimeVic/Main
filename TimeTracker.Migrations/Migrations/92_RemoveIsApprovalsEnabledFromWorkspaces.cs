using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(92)]
public class _92_RemoveIsApprovalsEnabledFromWorkspaces : MyMigration
{
    public override void Up()
    {
        Delete.Column("is_approvals_enabled").FromTable("workspaces");
        base.Up();
    }

    public override void Down()
    {
        Alter.Table("workspaces")
            .AddColumn("is_approvals_enabled").AsBoolean().NotNullable().WithDefaultValue(false);
        base.Down();
    }
}

