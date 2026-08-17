using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(88)]
public class _88_AddTimeEntryAutoStopFields : MyMigration
{
    public override void Up()
    {
        Alter.Table("time_entries")
            .AddColumn("is_autostopped").AsBoolean().NotNullable().WithDefaultValue(false)
            .AddColumn("auto_stop_warning_sent_at").AsDateTime().Nullable();

        Execute.Sql("""
            CREATE INDEX "IX_time_entries_active_start_time"
            ON time_entries (start_time)
            WHERE end_time IS NULL AND is_marked_to_delete = false;
            """);

        base.Up();
    }
}
