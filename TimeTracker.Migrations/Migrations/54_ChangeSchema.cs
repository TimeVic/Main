using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(54)]
    public class _54_ChangeSchema : MyMigration
    {
        public override void Up()
        {
            Alter.Table("membership_access_types").ToSchema("enum");
            Alter.Table("notification_types").ToSchema("enum");
            Alter.Table("queue_channels").ToSchema("enum");
            Alter.Table("queue_statuses").ToSchema("enum");
            Alter.Table("stored_file_statuses").ToSchema("enum");
            Alter.Table("stored_file_types").ToSchema("enum");
            Alter.Table("task_priorities").ToSchema("enum");
            Alter.Table("task_statuses").ToSchema("enum");
            
            base.Up();
        }
    }
}
