using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(80)]
public class _80_CreateNoteNodeStoredFiles : MyMigration
{
    public override void Up()
    {
        Create.Table("note_node_stored_files")
            .WithColumn("note_node_id").AsGuid().NotNullable()
            .WithColumn("stored_file_id").AsGuid().NotNullable();

        Create.ForeignKey()
            .FromTable("note_node_stored_files").ForeignColumn("note_node_id")
            .ToTable("note_nodes").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_node_stored_files").ForeignColumn("stored_file_id")
            .ToTable("stored_files").PrimaryColumn("id");

        Create.Index().OnTable("note_node_stored_files").OnColumn("note_node_id");
        Create.Index().OnTable("note_node_stored_files").OnColumn("stored_file_id");
        Create.UniqueConstraint()
            .OnTable("note_node_stored_files")
            .Columns("note_node_id", "stored_file_id");

        base.Up();
    }
}
