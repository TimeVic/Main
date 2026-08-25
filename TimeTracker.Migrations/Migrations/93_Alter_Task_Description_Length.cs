using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(93)]
public class _93_Alter_Task_Description_Length : MyMigration
{
    public override void Up()
    {
        Alter.Table("tasks")
            .AlterColumn("description").AsString(30_000).Nullable();

        Alter.Table("task_history_items")
            .AlterColumn("description").AsString(30_000).Nullable();

        base.Up();
    }

    public override void Down()
    {
        Alter.Table("tasks")
            .AlterColumn("description").AsString(10_000).Nullable();

        Alter.Table("task_history_items")
            .AlterColumn("description").AsString(10_000).Nullable();

        base.Down();
    }
}
