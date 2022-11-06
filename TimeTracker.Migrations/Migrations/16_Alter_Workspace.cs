using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(16)]
    public class _16_Alter_Workspace : MyMigration
    {
        public override void Up()
        {
            ExecuteScriptByName("16_CreateWorkspaceMemberships");
            Rename.Column("user_id").OnTable("workspaces").To("created_user_id");

            base.Up();
        }
    }
}
