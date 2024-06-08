using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(43)]
    public class _43_CreateTable_UserJwtHistory : MyMigration
    {
        public override void Up()
        {
            Create.Table("user_jwt_tokens")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("access_token_id").AsInt64().NotNullable()
                    .ForeignKey("user_access_tokens", "id")
                .WithColumn("token").AsString(2056).NotNullable()
                .WithColumn("expiration_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();

            Delete.Column("last_jwt").FromTable("user_access_tokens");
            
            base.Up();
        }
    }
}
