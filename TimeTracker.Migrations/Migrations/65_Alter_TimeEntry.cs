using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(65)]
public class _65_Alter_TimeEntry : MyMigration
{
    public override void Up()
    {
        Alter.Table("tasks")
            .AddColumn("original_estimate").AsInt64().Nullable();

        base.Up();
    }
}
