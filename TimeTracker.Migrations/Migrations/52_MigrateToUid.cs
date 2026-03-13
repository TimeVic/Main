using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(52)]
    public class _52_MigrateToUid : MyMigration
    {
        public override void Up()
        {
            Create.ForeignKey().FromTable("payments").ForeignColumn("project_id").ToTable("projects").PrimaryColumn("id");
            Create.ForeignKey().FromTable("payments").ForeignColumn("client_id").ToTable("clients").PrimaryColumn("id");
            
            Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
            Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
            
            ExecuteScriptByName("52_MigrateToUid");
            
            base.Up();
        }
    }
}
