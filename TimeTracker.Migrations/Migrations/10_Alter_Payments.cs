using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(10)]
    public class _10_Alter_Payments : MyMigration
    {
        public override void Up()
        {
            Alter.Table("payments")
                .AddColumn("user_id").AsInt64().Nullable()
                .AddColumn("workspace_id").AsInt64().Nullable();
            
            Create.ForeignKey()
                .FromTable("payments").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.ForeignKey()
                .FromTable("payments").ForeignColumn("workspace_id")
                .ToTable("workspaces").PrimaryColumn("id");

            
            base.Up();
        }
    }
}
