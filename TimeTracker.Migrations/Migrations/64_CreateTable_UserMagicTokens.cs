using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(64)]
public class _64_CreateTable_UserMagicTokens : MyMigration
{
    public override void Up()
    {
        Create.Table("user_magic_tokens")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("token").AsString(256).NotNullable()
            .WithColumn("expiration_time").AsDateTime().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.ForeignKey()
            .FromTable("user_magic_tokens").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id");

        base.Up();
    }
}
