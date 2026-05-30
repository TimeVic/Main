using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(78)]
public class _78_CreateTable_UserSocialLogins : MyMigration
{
    public override void Up()
    {
        Create.Table("user_social_logins")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("google_id").AsString(512).Nullable()
            .WithColumn("google_access_token").AsCustom("text").Nullable()
            .WithColumn("google_refresh_token").AsCustom("text").Nullable()
            .WithColumn("google_connected_at").AsCustom("timestamp").Nullable()
            .WithColumn("facebook_id").AsString(512).Nullable()
            .WithColumn("facebook_access_token").AsCustom("text").Nullable()
            .WithColumn("facebook_refresh_token").AsCustom("text").Nullable()
            .WithColumn("facebook_connected_at").AsCustom("timestamp").Nullable()
            .WithColumn("apple_id").AsString(512).Nullable()
            .WithColumn("apple_access_token").AsCustom("text").Nullable()
            .WithColumn("apple_refresh_token").AsCustom("text").Nullable()
            .WithColumn("apple_connected_at").AsCustom("timestamp").Nullable()
            .WithColumn("created_at").AsCustom("timestamp").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsCustom("timestamp").Nullable()
            .WithColumn("deleted_at").AsCustom("timestamp").Nullable();

        Create.ForeignKey()
            .FromTable("user_social_logins").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id");

        Create.Index()
            .OnTable("user_social_logins")
            .OnColumn("user_id").Unique();

        Create.Index()
            .OnTable("user_social_logins")
            .OnColumn("google_id").Unique();

        Create.Index()
            .OnTable("user_social_logins")
            .OnColumn("facebook_id").Unique();

        Create.Index()
            .OnTable("user_social_logins")
            .OnColumn("apple_id").Unique();

        base.Up();
    }
}
