using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(68)]
    public class _68_Rename_WorkspaceMembers : MyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                do $$
                declare
                    item record;
                    new_name text;
                begin
                    if to_regclass('workspace_memberships') is not null
                       and to_regclass('workspace_members') is null then
                        alter table workspace_memberships rename to workspace_members;
                    end if;

                    if to_regclass('workspace_membership_project_accesses') is not null then
                        if exists (
                            select 1
                            from information_schema.columns
                            where table_schema = current_schema()
                              and table_name = 'workspace_membership_project_accesses'
                              and column_name = 'workspace_membership_id'
                        ) then
                            alter table workspace_membership_project_accesses
                                rename column workspace_membership_id to workspace_member_id;
                        end if;

                        if to_regclass('workspace_member_project_accesses') is null then
                            alter table workspace_membership_project_accesses
                                rename to workspace_member_project_accesses;
                        end if;
                    end if;

                    if to_regclass('workspace_member_project_accesses') is not null
                       and exists (
                           select 1
                           from information_schema.columns
                           where table_schema = current_schema()
                             and table_name = 'workspace_member_project_accesses'
                             and column_name = 'workspace_membership_id'
                       ) then
                        alter table workspace_member_project_accesses
                            rename column workspace_membership_id to workspace_member_id;
                    end if;

                    for item in
                        select con.conname, rel.relname
                        from pg_constraint con
                                 join pg_class rel on rel.oid = con.conrelid
                                 join pg_namespace nsp on nsp.oid = rel.relnamespace
                        where nsp.nspname = current_schema()
                          and con.conname like '%membership%'
                    loop
                        new_name := replace(item.conname, 'workspace_memberships', 'workspace_members');
                        new_name := replace(new_name, 'workspace_membership_project_accesses', 'workspace_member_project_accesses');
                        new_name := replace(new_name, 'workspace_membership_id', 'workspace_member_id');
                        new_name := replace(new_name, 'workspace_memberships_id', 'workspace_members_id');

                        if new_name <> item.conname then
                            execute format(
                                'alter table %I rename constraint %I to %I',
                                item.relname,
                                item.conname,
                                new_name
                            );
                        end if;
                    end loop;

                    for item in
                        select cls.relname
                        from pg_class cls
                                 join pg_namespace nsp on nsp.oid = cls.relnamespace
                        where nsp.nspname = current_schema()
                          and cls.relkind = 'i'
                          and cls.relname like '%membership%'
                    loop
                        new_name := replace(item.relname, 'workspace_memberships', 'workspace_members');
                        new_name := replace(new_name, 'workspace_membership_project_accesses', 'workspace_member_project_accesses');
                        new_name := replace(new_name, 'workspace_membership_id', 'workspace_member_id');

                        if new_name <> item.relname and to_regclass(new_name) is null then
                            execute format(
                                'alter index %I rename to %I',
                                item.relname,
                                new_name
                            );
                        end if;
                    end loop;
                end $$;
            ");

            base.Up();
        }
    }
}
