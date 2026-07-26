using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(84)]
public class _84_RemoveProjectsIsArchived : MyMigration
{
    public override void Up()
    {
        Execute.Sql("UPDATE projects SET deleted_at = NOW() WHERE is_archived = TRUE AND deleted_at IS NULL;");
        Delete.Column("is_archived").FromTable("projects");
        base.Up();
    }
}
