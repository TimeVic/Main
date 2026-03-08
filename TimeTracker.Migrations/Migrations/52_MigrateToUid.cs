using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(52)]
    public class _52_MigrateToUid : MyMigration
    {
        public override void Up()
        {
            ExecuteScriptByName("52_MigrateToUid");
            
            base.Up();
        }
    }
}
