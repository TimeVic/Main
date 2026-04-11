using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(63)]
    public class _63_Alter_TimeEntry : MyMigration
    {
        public override void Up()
        {
            Alter.Table("time_entries")
                .AddColumn("time_zone").AsString().Nullable();
            
            Execute.Sql(@"
                UPDATE time_entries t
                SET time_zone = s.time_zone
                FROM (
                    SELECT id, time_zone
                    FROM workspaces
                ) AS s
                WHERE t.workspace_id = s.id;
            ");
            
            Alter.Table("time_entries")
                .AlterColumn("time_zone").AsString().NotNullable();
            
            base.Up();
        }
    }
}
