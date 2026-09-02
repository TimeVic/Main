using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(95)]
public class _95_MigrateTaskInProgressStatus : MyMigration
{
    public override void Up()
    {
        Execute.Sql("UPDATE tasks SET status = 2 WHERE status = 3;");
        Execute.Sql("UPDATE task_history_items SET status = 2 WHERE status = 3;");
        Execute.Sql("DELETE FROM enum.task_statuses WHERE id = 3;");
        Execute.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_time_entries_active_task"
            ON time_entries (internal_task_id)
            WHERE end_time IS NULL AND is_marked_to_delete = false;
        """);

        base.Up();
    }

    public override void Down()
    {
        Execute.Sql("""
            DROP INDEX IF EXISTS "IX_time_entries_active_task";
        """);
        Execute.Sql("""
            INSERT INTO enum.task_statuses (id, name) VALUES (3, 'InProgress') ON CONFLICT DO NOTHING;
        """);

        base.Down();
    }
}
