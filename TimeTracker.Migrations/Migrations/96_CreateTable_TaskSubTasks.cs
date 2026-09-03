using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(96)]
public class _96_CreateTable_TaskSubTasks : MyMigration
{
    public override void Up()
    {
        Create.Table("task_sub_tasks")
            .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable()
            .WithColumn("task_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(512).NotNullable()
            .WithColumn("is_completed").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("position_index").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("created_at").AsCustom("timestamp").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp").Nullable();

        Create.ForeignKey()
            .FromTable("task_sub_tasks").ForeignColumn("task_id")
            .ToTable("tasks").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index()
            .OnTable("task_sub_tasks")
            .OnColumn("task_id");

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("task_sub_tasks");

        base.Down();
    }
}
