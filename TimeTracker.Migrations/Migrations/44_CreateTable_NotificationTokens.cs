using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(44)]
    public class _44_CreateTable_NotificationTokens : MyMigration
    {
        public override void Up()
        {
            Create.Table("user_notification_tokens")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable()
                .ForeignKey("users", "id")
                .WithColumn("token").AsString(1024).NotNullable().Unique()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Alter.Table("tasks")
                .AddColumn("is_reminder_enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .AddColumn("remind_time").AsCustom("timestamptz").Nullable()
                .AddColumn("reminded_at").AsCustom("timestamptz").Nullable();
            
            base.Up();
        }
    }
}
