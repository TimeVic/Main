using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(90)]
public class _90_MigrateNoteVisibilityByWorkspaceMode : MyMigration
{
    public override void Up()
    {
        // For Team workspaces (mode = 2), migrate all Private (1) notes to Workspace (2)
        Execute.Sql("""
            UPDATE note_nodes nn
            SET visibility = 2
            FROM workspaces w
            WHERE nn.workspace_id = w.id
              AND w.mode = 2
              AND nn.visibility = 1;
            """);

        // For Solo workspaces (mode = 1), ensure all Workspace (2) notes are set to Private (1)
        Execute.Sql("""
            UPDATE note_nodes nn
            SET visibility = 1
            FROM workspaces w
            WHERE nn.workspace_id = w.id
              AND w.mode = 1
              AND nn.visibility = 2;
            """);

        base.Up();
    }
}
