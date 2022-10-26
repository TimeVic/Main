using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(15)]
    public class _15_Alter_MembershipAccess : MyMigration
    {
        public override void Up()
        {
            Alter.Table("workspace_membership_project_accesses")
                .AddColumn("hourly_rate").AsDecimal(8, 2).Nullable();

            base.Up();
        }
    }
}
