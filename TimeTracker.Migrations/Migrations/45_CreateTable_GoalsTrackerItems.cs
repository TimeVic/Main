using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(45)]
    public class _45_CreateTable_GoalsTrackerItems : MyMigration
    {
        public override void Up()
        {
            Create.Table("goals_trackers")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("year").AsInt32().NotNullable()
                .WithColumn("month").AsInt32().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("workspace_id").AsInt64().NotNullable().ForeignKey("workspaces", "id")
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("goals_tracker_items")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("goals_tracker_id").AsInt64().NotNullable().ForeignKey("goals_trackers", "id")
                .WithColumn("name").AsString(1024).NotNullable()
                .WithColumn("number_of_times").AsInt32().NotNullable()
                .WithColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("goals_tracker_completion_markers")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("goals_tracker_item_id").AsInt64().NotNullable().ForeignKey("goals_tracker_items", "id")
                .WithColumn("day_of_month").AsInt32().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("goals_tracker_notes")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("goals_tracker_id").AsInt64().NotNullable().ForeignKey("goals_trackers", "id")
                .WithColumn("text").AsString(5064).NotNullable()
                .WithColumn("is_archived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
