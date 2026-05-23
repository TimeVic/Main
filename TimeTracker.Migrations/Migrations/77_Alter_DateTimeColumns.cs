using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(77)]
public class _77_Alter_DateTimeColumns : MyMigration
{
    public override void Up()
    {
        // Convert remaining timestamptz columns to timestamp for consistent DateTime storage.
        Execute.Sql("""
            DO $$
            DECLARE
                column_record record;
            BEGIN
                FOR column_record IN
                    SELECT table_schema, table_name, column_name
                    FROM information_schema.columns
                    WHERE data_type = 'timestamp with time zone'
                      AND table_schema NOT IN ('pg_catalog', 'information_schema')
                    ORDER BY table_schema, table_name, ordinal_position
                LOOP
                    EXECUTE format(
                        'ALTER TABLE %I.%I ALTER COLUMN %I TYPE timestamp USING %I AT TIME ZONE ''UTC''',
                        column_record.table_schema,
                        column_record.table_name,
                        column_record.column_name,
                        column_record.column_name
                    );
                END LOOP;
            END $$;
            """);

        base.Up();
    }
}
