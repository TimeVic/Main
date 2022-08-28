using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(1)]
    public class _1_Init : MyMigration
    {
        public override void Up()
        {
            ExecuteScriptByName("1_create_types");
            
            Create.Table("users")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_name").AsString(200).Unique()
                .WithColumn("email").AsString(200).Unique()
                .WithColumn("verification_token").AsString(512).Nullable()
                .WithColumn("verification_time").AsDateTime().Nullable()
                .WithColumn("password_hash").AsBinary()
                .WithColumn("password_salt").AsBinary()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("workspaces")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64()
                .WithColumn("name").AsString(200)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("workspaces").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.Table("clients")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64()
                .WithColumn("name").AsString(200)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("clients").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.Table("projects")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64()
                .WithColumn("client_id").AsInt64().Nullable()
                .WithColumn("name").AsString(200)
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("projects").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("projects").ForeignColumn("client_id")
                .ToTable("clients").PrimaryColumn("id");
            
            Create.Table("time_entries")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("workspace_id").AsInt64()
                .WithColumn("project_id").AsInt64().Nullable()
                .WithColumn("description").AsString(1000)
                .WithColumn("hourly_rate").AsDecimal(8, 2).Nullable()
                .WithColumn("is_billable").AsBoolean().WithDefaultValue(false)
                
                .WithColumn("start_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("end_time").AsCustom("timestamptz").NotNullable()
                
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable();
            
            Create.ForeignKey()
                .FromTable("time_entries").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("time_entries").ForeignColumn("workspace_id")
                .ToTable("projects").PrimaryColumn("id");
            
            base.Up();
        }
    }
}
