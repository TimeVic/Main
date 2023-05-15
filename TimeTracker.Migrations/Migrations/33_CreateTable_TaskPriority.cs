using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(33)]
    public class _33_CreateTable_TaskPriority : MyMigration
    {
        public override void Up()
        {
            Create.Table("task_priorities")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();
            
            Insert.IntoTable("task_priorities")
                .Row(new {id = 1, name = "Urgent"})
                .Row(new {id = 2, name = "High"})
                .Row(new {id = 3, name = "Medium"})
                .Row(new {id = 4, name = "Low"});

            Alter.Table("tasks")
                .AddColumn("priority").AsInt32().NotNullable().WithDefaultValue("4")
                .ForeignKey("task_priorities", "id");

            base.Up();
        }
    }
}
