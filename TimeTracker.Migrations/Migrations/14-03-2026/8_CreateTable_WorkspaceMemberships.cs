using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(8)]
    public class _8_CreateTable_WorkspaceMemberships : MyMigration
    {
        public override void Up()
        {
            Create.Table("membership_access_types")
                .WithColumn("id").AsInt16().PrimaryKey()
                .WithColumn("name").AsString(200).Unique();

            Insert.IntoTable("membership_access_types")
                .Row(new {id = 1, name = "User"})
                .Row(new {id = 2, name = "Manager"});
            
            Create.Table("workspace_memberships")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64().NotNullable()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("membership_access_type_id").AsInt16().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("workspace_memberships").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("workspace_memberships").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("workspace_memberships").ForeignColumn("membership_access_type_id")
                .ToTable("membership_access_types").PrimaryColumn("id");
            
            Create.UniqueConstraint()
                .OnTable("workspace_memberships").Columns("workspace_id", "user_id");
            
            Create.Table("workspace_membership_project_accesses")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_membership_id").AsInt64().NotNullable()
                .WithColumn("project_id").AsInt64().NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();

            Create.ForeignKey()
                .FromTable("workspace_membership_project_accesses").ForeignColumn("workspace_membership_id")
                .ToTable("workspace_memberships").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("workspace_membership_project_accesses").ForeignColumn("project_id")
                .ToTable("projects").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
