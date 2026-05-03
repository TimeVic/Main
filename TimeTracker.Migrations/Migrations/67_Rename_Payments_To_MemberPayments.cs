using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(67)]
    public class _67_Rename_Payments_To_MemberPayments : MyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
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
                          and rel.relname = 'payments'
                          and con.contype = 'f'
                          and (
                              pg_get_constraintdef(con.oid) like '%user_id%'
                              or pg_get_constraintdef(con.oid) like '%workspace_id%'
                          )
                    loop
                        execute format(
                            'alter table %I.%I drop constraint %I',
                            current_schema(),
                            'payments',
                            constraint_record.conname
                        );
                    end loop;
                end $$;

                alter table if exists payments rename to member_payments;
                alter table member_payments rename column user_id to member_id;

                update member_payments mp
                set member_id = wm.id
                from workspace_memberships wm
                where wm.workspace_id = mp.workspace_id
                  and wm.user_id = mp.member_id;

                alter table member_payments alter column member_id set not null;

                alter table member_payments
                    add constraint fk_member_payments_member_id_workspace_memberships_id
                    foreign key (member_id) references workspace_memberships(id);

                alter table member_payments drop column workspace_id;
            ");

            base.Up();
        }
    }
}
