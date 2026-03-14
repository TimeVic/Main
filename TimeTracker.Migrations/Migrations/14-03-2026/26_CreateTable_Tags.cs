using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(26)]
    public class _26_CreateTable_Tags : MyMigration
    {
        public override void Up()
        {
            Create.Table("tags")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("name").AsString(200).NotNullable()
                .WithColumn("color").AsString(30).Nullable()
                .WithColumn("workspace_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();

            Create.ForeignKey()
                .FromTable("tags").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.Table("task_tags")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("task_id").AsInt64().NotNullable()
                .WithColumn("tag_id").AsInt64().NotNullable();
            
            Create.ForeignKey()
                .FromTable("task_tags").ForeignColumn("task_id")
                .ToTable("tasks").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("task_tags").ForeignColumn("tag_id")
                .ToTable("tags").PrimaryColumn("id");
            
            Create.Table("time_entry_tags")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("time_entry_id").AsInt64().NotNullable()
                .WithColumn("tag_id").AsInt64().NotNullable();
            
            Create.ForeignKey()
                .FromTable("time_entry_tags").ForeignColumn("time_entry_id")
                .ToTable("time_entries").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("time_entry_tags").ForeignColumn("tag_id")
                .ToTable("tags").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
