using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(79)]
public class _79_CreateNoteContents : MyMigration
{
    public override void Up()
    {
        Create.Table("note_contents")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("note_node_id").AsGuid().NotNullable()
            .WithColumn("markdown_content").AsString(5_000_000).NotNullable()
            .WithColumn("legacy_history_id").AsGuid().Nullable()
            .WithColumn("created_at").AsCustom("timestamp").NotNullable();

        Alter.Table("note_nodes")
            .AddColumn("last_content_id").AsGuid().Nullable();

        Alter.Table("note_node_history")
            .AddColumn("note_content_id").AsGuid().Nullable();

        Execute.Sql("""
            INSERT INTO note_contents (note_node_id, markdown_content, created_at)
            SELECT id, COALESCE(markdown_content, ''), COALESCE(updated_at, created_at)
            FROM note_nodes
            WHERE type = 2;
            """);

        Execute.Sql("""
            UPDATE note_nodes node
            SET last_content_id = content.id
            FROM note_contents content
            WHERE content.note_node_id = node.id
              AND content.created_at = COALESCE(node.updated_at, node.created_at);
            """);

        Execute.Sql("""
            INSERT INTO note_contents (note_node_id, markdown_content, legacy_history_id, created_at)
            SELECT note_node_id, COALESCE(markdown_content, ''), id, created_at
            FROM note_node_history;
            """);

        Execute.Sql("""
            UPDATE note_node_history history
            SET note_content_id = content.id
            FROM note_contents content
            WHERE content.legacy_history_id = history.id;
            """);

        Alter.Table("note_node_history")
            .AlterColumn("note_content_id").AsGuid().NotNullable();

        Create.ForeignKey()
            .FromTable("note_contents").ForeignColumn("note_node_id")
            .ToTable("note_nodes").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("last_content_id")
            .ToTable("note_contents").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_node_history").ForeignColumn("note_content_id")
            .ToTable("note_contents").PrimaryColumn("id");

        Create.Index().OnTable("note_contents").OnColumn("note_node_id");
        Create.Index().OnTable("note_contents")
            .OnColumn("note_node_id").Ascending()
            .OnColumn("created_at").Ascending();
        Create.Index().OnTable("note_nodes").OnColumn("last_content_id");
        Create.Index().OnTable("note_node_history").OnColumn("note_content_id");

        Delete.Column("legacy_history_id").FromTable("note_contents");
        Delete.Column("markdown_content").FromTable("note_nodes");
        Delete.Column("markdown_content").FromTable("note_node_history");

        base.Up();
    }
}
