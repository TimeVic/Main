using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(42)]
    public class _42_CreateTable_UserAccessToken : MyMigration
    {
        public override void Up()
        {
            Create.Table("user_access_tokens")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable()
                    .ForeignKey("users", "id")
                .WithColumn("token").AsString(200).NotNullable()
                .WithColumn("last_jwt").AsString(512).NotNullable()
                .WithColumn("expiration_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
