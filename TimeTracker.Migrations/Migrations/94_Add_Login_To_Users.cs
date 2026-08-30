using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(94)]
public class _94_Add_Login_To_Users : MyMigration
{
    public override void Up()
    {
        Alter.Table("users")
            .AddColumn("login").AsString(100).Nullable();

        Execute.Sql("""
            WITH base_logins AS (
                SELECT 
                    id,
                    CASE 
                        WHEN length(TRIM(BOTH '_' FROM REGEXP_REPLACE(LOWER(SPLIT_PART(email, '@', 1)), '[^a-z0-9]+', '_', 'g'))) < 3 
                        THEN RPAD(COALESCE(NULLIF(TRIM(BOTH '_' FROM REGEXP_REPLACE(LOWER(SPLIT_PART(email, '@', 1)), '[^a-z0-9]+', '_', 'g')), ''), 'usr'), 3, '0')
                        ELSE TRIM(BOTH '_' FROM REGEXP_REPLACE(LOWER(SPLIT_PART(email, '@', 1)), '[^a-z0-9]+', '_', 'g'))
                    END AS clean_login
                FROM users
            ),
            numbered_logins AS (
                SELECT 
                    id,
                    clean_login,
                    ROW_NUMBER() OVER (PARTITION BY clean_login ORDER BY id) as rn
                FROM base_logins
            )
            UPDATE users u
            SET login = CASE 
                WHEN nl.rn = 1 THEN nl.clean_login 
                ELSE nl.clean_login || '_' || (nl.rn - 1) 
            END
            FROM numbered_logins nl
            WHERE u.id = nl.id;
        """);

        Alter.Table("users")
            .AlterColumn("login").AsString(100).NotNullable();

        Create.UniqueConstraint().OnTable("users").Column("login");

        base.Up();
    }

    public override void Down()
    {
        Delete.UniqueConstraint().FromTable("users").Column("login");
        Delete.Column("login").FromTable("users");

        base.Down();
    }
}
