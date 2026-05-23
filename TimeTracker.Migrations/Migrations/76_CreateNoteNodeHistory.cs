using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(76)]
public class _76_CreateNoteNodeHistory : MyMigration
{
    public override void Up()
    {
        Create.Table("note_node_history")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("note_node_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(200).NotNullable()
            .WithColumn("markdown_content").AsString(5_000_000).Nullable()
            .WithColumn("sort_order").AsInt32().NotNullable()
            .WithColumn("created_at").AsCustom("timestamptz").NotNullable();

        Create.ForeignKey()
            .FromTable("note_node_history").ForeignColumn("note_node_id")
            .ToTable("note_nodes").PrimaryColumn("id");

        Create.Index().OnTable("note_node_history").OnColumn("note_node_id");
        Create.Index().OnTable("note_node_history")
            .OnColumn("note_node_id").Ascending()
            .OnColumn("created_at").Ascending();

        base.Up();
    }
}
