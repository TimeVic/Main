using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(13)]
    public class _13_Alter_Users : MyMigration
    {
        public override void Up()
        {
            Alter.Table("users")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("workspaces")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("clients")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("projects")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("time_entries")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("queues")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("queues")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("payments")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable()
                .AlterColumn("payment_time").AsDateTime2().NotNullable();
            
            Alter.Table("workspace_setting_clickups")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();
            
            Alter.Table("workspace_memberships")
                .AlterColumn("create_time").AsDateTime2().NotNullable()
                .AlterColumn("update_time").AsDateTime2().NotNullable();

            base.Up();
        }
    }
}
