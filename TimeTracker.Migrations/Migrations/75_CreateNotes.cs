using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(75)]
public class _75_CreateNotes : MyMigration
{
    public override void Up()
    {
        Create.Table("note_node_types").InSchema("enum")
            .WithColumn("id").AsInt16().PrimaryKey()
            .WithColumn("name").AsString(200).Unique().NotNullable();

        Insert.IntoTable("note_node_types").InSchema("enum")
            .Row(new { id = 1, name = "Folder" })
            .Row(new { id = 2, name = "Document" });

        Create.Table("note_visibilities").InSchema("enum")
            .WithColumn("id").AsInt16().PrimaryKey()
            .WithColumn("name").AsString(200).Unique().NotNullable();

        Insert.IntoTable("note_visibilities").InSchema("enum")
            .Row(new { id = 1, name = "Private" })
            .Row(new { id = 2, name = "Workspace" });

        Create.Table("note_link_entity_types").InSchema("enum")
            .WithColumn("id").AsInt16().PrimaryKey()
            .WithColumn("name").AsString(200).Unique().NotNullable();

        Insert.IntoTable("note_link_entity_types").InSchema("enum")
            .Row(new { id = 1, name = "Client" })
            .Row(new { id = 2, name = "Project" })
            .Row(new { id = 3, name = "Task" });

        Create.Table("note_nodes")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("workspace_id").AsGuid().NotNullable()
            .WithColumn("parent_id").AsGuid().Nullable()
            .WithColumn("type").AsInt16().NotNullable()
            .WithColumn("title").AsString(200).NotNullable()
            .WithColumn("markdown_content").AsString(5_000_000).Nullable()
            .WithColumn("sort_order").AsInt32().NotNullable()
            .WithColumn("visibility").AsInt16().NotNullable()
            .WithColumn("created_by_user_id").AsGuid().NotNullable()
            .WithColumn("updated_by_user_id").AsGuid().Nullable()
            .WithColumn("archived_at").AsCustom("timestamptz").Nullable()
            .WithColumn("created_at").AsCustom("timestamptz").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamptz").Nullable();

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("workspace_id")
            .ToTable("workspaces").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("parent_id")
            .ToTable("note_nodes").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("type")
            .ToTable("note_node_types").InSchema("enum").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("visibility")
            .ToTable("note_visibilities").InSchema("enum").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("created_by_user_id")
            .ToTable("users").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_nodes").ForeignColumn("updated_by_user_id")
            .ToTable("users").PrimaryColumn("id");

        Create.Index().OnTable("note_nodes").OnColumn("workspace_id");
        Create.Index().OnTable("note_nodes").OnColumn("workspace_id").Ascending().OnColumn("parent_id").Ascending();
        Create.Index().OnTable("note_nodes").OnColumn("workspace_id").Ascending().OnColumn("type").Ascending();
        Create.Index().OnTable("note_nodes").OnColumn("workspace_id").Ascending().OnColumn("archived_at").Ascending();
        Create.Index().OnTable("note_nodes")
            .OnColumn("workspace_id").Ascending()
            .OnColumn("parent_id").Ascending()
            .OnColumn("sort_order").Ascending();

        Create.Table("note_links")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("workspace_id").AsGuid().NotNullable()
            .WithColumn("note_node_id").AsGuid().NotNullable()
            .WithColumn("entity_type").AsInt16().NotNullable()
            .WithColumn("entity_id").AsGuid().NotNullable()
            .WithColumn("created_by_user_id").AsGuid().NotNullable()
            .WithColumn("created_at").AsCustom("timestamptz").NotNullable();

        Create.ForeignKey()
            .FromTable("note_links").ForeignColumn("workspace_id")
            .ToTable("workspaces").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_links").ForeignColumn("note_node_id")
            .ToTable("note_nodes").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_links").ForeignColumn("entity_type")
            .ToTable("note_link_entity_types").InSchema("enum").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("note_links").ForeignColumn("created_by_user_id")
            .ToTable("users").PrimaryColumn("id");

        Create.Index().OnTable("note_links")
            .OnColumn("workspace_id").Ascending()
            .OnColumn("entity_type").Ascending()
            .OnColumn("entity_id").Ascending();
        Create.Index().OnTable("note_links").OnColumn("note_node_id");
        Create.UniqueConstraint()
            .OnTable("note_links")
            .Columns("note_node_id", "entity_type", "entity_id");

        base.Up();
    }
}
