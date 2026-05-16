using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(74)]
public class _74_CreateLanguagesAndUserPreferences : MyMigration
{
    public override void Up()
    {
        Create.Table("languages")
            .WithColumn("id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("code").AsString(10).NotNullable().Unique()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();

        Execute.Sql(@"
            INSERT INTO languages (name, code)
            VALUES
                ('English', 'en'),
                ('Українська', 'uk-UA')
            ON CONFLICT (code) DO NOTHING;
        ");

        Alter.Table("users")
            .AddColumn("selected_workspace_id").AsGuid().Nullable()
                .ForeignKey("workspaces", "id")
            .AddColumn("language_id").AsGuid().Nullable()
                .ForeignKey("languages", "id");

        Execute.Sql(@"
            UPDATE users
            SET language_id = (SELECT id FROM languages WHERE code = 'en')
            WHERE language_id IS NULL;
        ");

        Alter.Table("users")
            .AlterColumn("language_id").AsGuid().NotNullable();

        Create.Index()
            .OnTable("users")
            .OnColumn("selected_workspace_id");

        Create.Index()
            .OnTable("users")
            .OnColumn("language_id");

        base.Up();
    }
}
