using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(72)]
public class _72_Alter_StoredFiles_RemoveUploadPayload : MyMigration
{
    public override void Up()
    {
        Execute.Sql(@"
DO $$
DECLARE
    constraint_name text;
BEGIN
    FOR constraint_name IN
        SELECT tc.constraint_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name
            AND tc.table_schema = kcu.table_schema
        WHERE tc.constraint_type = 'FOREIGN KEY'
            AND tc.table_schema = current_schema()
            AND tc.table_name = 'stored_files'
            AND kcu.column_name = 'status'
    LOOP
        EXECUTE format('ALTER TABLE stored_files DROP CONSTRAINT %I', constraint_name);
    END LOOP;
END $$;
");

        Delete.Column("data_to_upload").FromTable("stored_files");
        Delete.Column("uploading_error").FromTable("stored_files");
        Delete.Column("status").FromTable("stored_files");

        Execute.Sql("drop table if exists enum.stored_file_statuses;");
        Execute.Sql("drop table if exists enum.stored_file_status;");

        base.Up();
    }
}
