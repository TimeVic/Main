using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(66)]
    public class _66_Alter_Tasks : MyMigration
    {
        public override void Up()
        {
            Create.Table("external_source_types").InSchema("enum")
                .WithColumn("id").AsInt16().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();

            Insert.IntoTable("external_source_types").InSchema("enum")
                .Row(new { id = 1, name = "Manual" })
                .Row(new { id = 2, name = "Jira" })
                .Row(new { id = 3, name = "ClickUp" })
                .Row(new { id = 4, name = "Redmine" });

            Alter.Table("tasks")
                .AddColumn("external_source_type").AsInt16().NotNullable().WithDefaultValue(1);

            Create.ForeignKey().FromTable("tasks").ForeignColumn("external_source_type")
                .ToTable("external_source_types").InSchema("enum").PrimaryColumn("id");

            Execute.Sql(@"
                update tasks
                set external_source_type = 2
                where external_task_id is not null
                  and length(trim(external_task_id)) > 0
            ");

            base.Up();
        }
    }
}
