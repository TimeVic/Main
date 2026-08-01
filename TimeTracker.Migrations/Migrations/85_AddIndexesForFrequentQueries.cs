using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(85)]
public class _85_AddIndexesForFrequentQueries : MyMigration
{
    public override void Up()
    {
        // Supports dashboard authorization checks and list queries that filter by foreign keys.
        Create.Index()
            .OnTable("workspace_members")
            .OnColumn("user_id").Ascending()
            .OnColumn("workspace_id").Ascending();

        Create.Index()
            .OnTable("workspace_member_project_accesses")
            .OnColumn("workspace_member_id").Ascending()
            .OnColumn("project_id").Ascending();

        Execute.Sql("""
            CREATE INDEX "IX_time_entries_active_workspace_id_user_id"
            ON time_entries (workspace_id, user_id)
            WHERE end_time IS NULL;
            """);

        Execute.Sql("""
            CREATE INDEX "IX_time_entries_workspace_id_start_time"
            ON time_entries (workspace_id, start_time DESC)
            WHERE is_marked_to_delete = false;
            """);

        Execute.Sql("""
            CREATE INDEX "IX_time_entries_internal_task_id_tracked"
            ON time_entries (internal_task_id)
            WHERE end_time IS NOT NULL AND is_marked_to_delete = false;
            """);

        Create.Index()
            .OnTable("clients")
            .OnColumn("workspace_id").Ascending()
            .OnColumn("name").Ascending();

        Create.Index()
            .OnTable("projects")
            .OnColumn("client_id").Ascending()
            .OnColumn("name").Ascending();

        Create.Index()
            .OnTable("task_lists")
            .OnColumn("project_id").Ascending()
            .OnColumn("is_archived").Ascending()
            .OnColumn("name").Ascending();

        Create.Index()
            .OnTable("tasks")
            .OnColumn("task_list_id").Ascending()
            .OnColumn("is_archived").Ascending()
            .OnColumn("position_index").Ascending();

        Create.Index()
            .OnTable("tasks")
            .OnColumn("task_id").Ascending();

        Create.Index()
            .OnTable("member_payments")
            .OnColumn("member_id").Ascending()
            .OnColumn("payment_time").Descending();

        Create.Index()
            .OnTable("client_payments")
            .OnColumn("client_id").Ascending()
            .OnColumn("payment_time").Descending();

        Create.Index()
            .OnTable("notifications")
            .OnColumn("receiver_user_id").Ascending()
            .OnColumn("workspace_id").Ascending()
            .OnColumn("created_at").Descending();

        Execute.Sql("""
            CREATE INDEX "IX_notifications_unread_receiver_user_id_workspace_id"
            ON notifications (receiver_user_id, workspace_id)
            WHERE is_read = false;
            """);

        Create.Index()
            .OnTable("tags")
            .OnColumn("workspace_id").Ascending();

        Create.Index()
            .OnTable("task_tags")
            .OnColumn("task_id").Ascending();

        Create.Index()
            .OnTable("time_entry_tags")
            .OnColumn("time_entry_id").Ascending();

        base.Up();
    }
}
