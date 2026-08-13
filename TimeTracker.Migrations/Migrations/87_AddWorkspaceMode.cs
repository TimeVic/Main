using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(87)]
public class _87_AddWorkspaceMode : MyMigration
{
    public override void Up()
    {
        Create.Table("workspace_modes").InSchema("enum")
            .WithColumn("id").AsInt16().PrimaryKey()
            .WithColumn("name").AsString(200).Unique().NotNullable();

        Insert.IntoTable("workspace_modes").InSchema("enum")
            .Row(new { id = 1, name = "Solo" })
            .Row(new { id = 2, name = "Team" });

        Alter.Table("workspaces")
            .AddColumn("mode").AsInt16().Nullable();

        Create.ForeignKey().FromTable("workspaces").ForeignColumn("mode")
            .ToTable("workspace_modes").InSchema("enum").PrimaryColumn("id");

        base.Up();
    }

    public override void Down()
    {
        Delete.ForeignKey().FromTable("workspaces").ForeignColumn("mode");

        Delete.Column("mode").FromTable("workspaces");

        Delete.Table("workspace_modes").InSchema("enum");

        base.Down();
    }
}
