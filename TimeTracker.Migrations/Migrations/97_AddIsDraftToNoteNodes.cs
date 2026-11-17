using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(97)]
public class _97_AddIsDraftToNoteNodes : MyMigration
{
    public override void Up()
    {
        Alter.Table("note_nodes")
            .AddColumn("is_draft").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Index()
            .OnTable("note_nodes")
            .OnColumn("workspace_id").Ascending()
            .OnColumn("is_draft").Ascending();

        base.Up();
    }

    public override void Down()
    {
        Delete.Column("is_draft").FromTable("note_nodes");

        base.Down();
    }
}
