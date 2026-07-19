using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(81)]
public class _81_RemoveOrphanedTaskStoredFiles : MyMigration
{
    public override void Up()
    {
        // Remove legacy attachment links that reference deleted stored files.
        Execute.Sql(@"
            DELETE FROM task_stored_files task_stored_file
            WHERE NOT EXISTS (
                SELECT 1
                FROM stored_files stored_file
                WHERE stored_file.id = task_stored_file.stored_file_id
            );

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint constraint_info
                    WHERE constraint_info.conrelid = 'task_stored_files'::regclass
                      AND constraint_info.contype = 'f'
                      AND constraint_info.confrelid = 'stored_files'::regclass
                      AND constraint_info.conkey = ARRAY[
                          (
                              SELECT attribute_info.attnum
                              FROM pg_attribute attribute_info
                              WHERE attribute_info.attrelid = 'task_stored_files'::regclass
                                AND attribute_info.attname = 'stored_file_id'
                                AND NOT attribute_info.attisdropped
                          )
                      ]
                ) THEN
                    ALTER TABLE task_stored_files
                        ADD CONSTRAINT fk_task_stored_files_stored_file_id
                        FOREIGN KEY (stored_file_id) REFERENCES stored_files(id);
                END IF;
            END $$;
        ");

        base.Up();
    }
}
