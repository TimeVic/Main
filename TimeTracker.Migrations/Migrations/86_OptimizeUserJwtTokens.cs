using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(86)]
public class _86_OptimizeUserJwtTokens : MyMigration
{
    public override void Up()
    {
        // JWT refresh validates tokens by their access token and removes expired history.
        Create.Index()
            .OnTable("user_access_tokens")
            .OnColumn("token");

        Create.Index()
            .OnTable("user_jwt_tokens")
            .OnColumn("access_token_id")
            .Ascending()
            .OnColumn("expiration_time")
            .Ascending();

        Execute.Sql("DELETE FROM user_jwt_tokens WHERE expiration_time < NOW();");

        base.Up();
    }
}
