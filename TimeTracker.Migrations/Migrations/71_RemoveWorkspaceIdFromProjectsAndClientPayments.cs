using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(71)]
public class _71_RemoveWorkspaceIdFromProjectsAndClientPayments : MyMigration
{
    public override void Up()
    {
        Execute.Sql(@"
INSERT INTO clients (id, workspace_id, name, created_at, updated_at)
SELECT md5(p.workspace_id::text || ':default-client')::uuid, p.workspace_id, 'Default', now(), now()
FROM projects p
WHERE p.client_id IS NULL
    AND p.workspace_id IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM clients c
        WHERE c.workspace_id = p.workspace_id
    )
GROUP BY p.workspace_id;

UPDATE projects p
SET client_id = (
    SELECT c.id
    FROM clients c
    WHERE c.workspace_id = p.workspace_id
    ORDER BY c.created_at, c.id
    LIMIT 1
)
WHERE p.client_id IS NULL
    AND p.workspace_id IS NOT NULL;

ALTER TABLE projects ALTER COLUMN client_id SET NOT NULL;
");

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
            AND tc.table_name = 'projects'
            AND kcu.column_name = 'workspace_id'
    LOOP
        EXECUTE format('ALTER TABLE projects DROP CONSTRAINT %I', constraint_name);
    END LOOP;
END $$;

ALTER TABLE projects DROP COLUMN IF EXISTS workspace_id;
");

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
            AND tc.table_name = 'client_payments'
            AND kcu.column_name = 'workspace_id'
    LOOP
        EXECUTE format('ALTER TABLE client_payments DROP CONSTRAINT %I', constraint_name);
    END LOOP;
END $$;

ALTER TABLE client_payments DROP COLUMN IF EXISTS workspace_id;
");

        base.Up();
    }
}
