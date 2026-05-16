using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations;

[Migration(73)]
public class _73_CreateTable_UserStoredFiles : MyMigration
{
    public override void Up()
    {
        Create.Table("user_stored_files")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid)
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("stored_file_id").AsGuid().NotNullable();
        
        Create.ForeignKey()
            .FromTable("user_stored_files").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id");
        
        Create.ForeignKey()
            .FromTable("user_stored_files").ForeignColumn("stored_file_id")
            .ToTable("stored_files").PrimaryColumn("id");
        
        Create.Index()
            .OnTable("user_stored_files")
            .OnColumn("user_id");
        
        Create.Index()
            .OnTable("user_stored_files")
            .OnColumn("stored_file_id");

        base.Up();
    }
}
