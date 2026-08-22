using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(91)]
public class _91_AddTimeEntryApprovals : MyMigration
{
    public override void Up()
    {
        Create.Table("time_entry_statuses").InSchema("enum")
            .WithColumn("id").AsInt16().PrimaryKey()
            .WithColumn("name").AsString(200).Unique().NotNullable();

        Insert.IntoTable("time_entry_statuses").InSchema("enum")
            .Row(new { id = 1, name = "Draft" })
            .Row(new { id = 2, name = "Pending" })
            .Row(new { id = 3, name = "Approved" })
            .Row(new { id = 4, name = "Rejected" });

        Alter.Table("workspaces")
            .AddColumn("is_approvals_enabled").AsBoolean().NotNullable().WithDefaultValue(false);

        Alter.Table("time_entries")
            .AddColumn("status").AsInt16().NotNullable().WithDefaultValue(3);

        Create.ForeignKey()
            .FromTable("time_entries").ForeignColumn("status")
            .ToTable("time_entry_statuses").InSchema("enum").PrimaryColumn("id");

        Execute.Sql("UPDATE time_entries SET status = 3;");

        Create.Table("time_entry_approvals")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("time_entry_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("created_at").AsCustom("timestamp").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp").Nullable();

        Create.ForeignKey()
            .FromTable("time_entry_approvals").ForeignColumn("time_entry_id")
            .ToTable("time_entries").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey()
            .FromTable("time_entry_approvals").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Table("time_entry_rejects")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("time_entry_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("reason").AsString(int.MaxValue).NotNullable()
            .WithColumn("created_at").AsCustom("timestamp").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp").Nullable();

        Create.ForeignKey()
            .FromTable("time_entry_rejects").ForeignColumn("time_entry_id")
            .ToTable("time_entries").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey()
            .FromTable("time_entry_rejects").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index()
            .OnTable("time_entry_approvals")
            .OnColumn("time_entry_id").Ascending();

        Create.Index()
            .OnTable("time_entry_rejects")
            .OnColumn("time_entry_id").Ascending();

        Create.Index()
            .OnTable("time_entries")
            .OnColumn("status").Ascending();

        base.Up();
    }

    public override void Down()
    {
        Delete.Table("time_entry_rejects");
        Delete.Table("time_entry_approvals");

        Delete.ForeignKey().FromTable("time_entries").ForeignColumn("status");
        Delete.Column("status").FromTable("time_entries");
        Delete.Column("is_approvals_enabled").FromTable("workspaces");

        Delete.Table("time_entry_statuses").InSchema("enum");

        base.Down();
    }
}
