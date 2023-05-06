using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(32)]
    public class _32_CreateTable_TaskStatuses : MyMigration
    {
        public override void Up()
        {
            Create.Table("task_statuses")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();
            
            Insert.IntoTable("task_statuses")
                .Row(new {id = 1, name = "Backlog"})
                .Row(new {id = 2, name = "ToDo"})
                .Row(new {id = 3, name = "InProgress"})
                .Row(new {id = 4, name = "Done"});

            Alter.Table("tasks")
                .AddColumn("status").AsInt32().NotNullable().WithDefaultValue("1")
                .ForeignKey("task_statuses", "id");
            
            Execute.Sql("update tasks set status = 4 where is_done = true");

            Delete.Column("is_done").FromTable("tasks");
            
            Alter.Table("task_history_items")
                .AddColumn("status").AsInt32().NotNullable().WithDefaultValue("1")
                .ForeignKey("task_statuses", "id");
            
            Execute.Sql("update task_history_items set status = 4 where is_done = true");
            
            Delete.Column("is_done").FromTable("task_history_items");
            
            base.Up();
        }
    }
}
