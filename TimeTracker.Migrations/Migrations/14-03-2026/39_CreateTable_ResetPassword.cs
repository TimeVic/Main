using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(39)]
    public class _39_CreateTable_ResetPassword : MyMigration
    {
        public override void Up()
        {
            Create.ForeignKey().FromTable("workspace_setting_jiras")
                .ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");
            Create.ForeignKey().FromTable("workspace_setting_jiras")
                .ForeignColumn("workspace_id").ToTable("workspaces").PrimaryColumn("id");
            
            Create.Table("user_reset_password_requests")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable()
                .ForeignKey("users", "id")
                .WithColumn("verification_token").AsString(1024).NotNullable()
                .WithColumn("expiration_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
