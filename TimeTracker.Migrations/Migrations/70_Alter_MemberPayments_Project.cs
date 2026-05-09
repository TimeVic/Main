using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(70)]
public class _70_Alter_MemberPayments_Project : MyMigration
{
    public override void Up()
    {
        Execute.Sql(@"
            update member_payments mp
            set project_id = (
                select p.id
                from projects p
                where p.client_id = mp.client_id
                order by p.created_at, p.id
                limit 1
            )
            where mp.project_id is null;

            alter table member_payments alter column project_id set not null;

            do $$
            declare
                constraint_record record;
            begin
                for constraint_record in
                    select con.conname
                    from pg_constraint con
                             inner join pg_class rel on rel.oid = con.conrelid
                             inner join pg_namespace nsp on nsp.oid = rel.relnamespace
                    where nsp.nspname = current_schema()
                      and rel.relname = 'member_payments'
                      and con.contype = 'f'
                      and pg_get_constraintdef(con.oid) like '%client_id%'
                loop
                    execute format(
                        'alter table %I.%I drop constraint %I',
                        current_schema(),
                        'member_payments',
                        constraint_record.conname
                    );
                end loop;
            end $$;

            alter table member_payments drop column client_id;
        ");

        base.Up();
    }
}
