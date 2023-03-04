using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(28)]
    public class _28_AlterTable_StoredFile : MyMigration
    {
        public override void Up()
        {
            Create.Table("stored_file_statuses")
                .WithColumn("id").AsInt16().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();
            
            Insert.IntoTable("stored_file_statuses")
                .Row(new {id = 1, name = "Pending"})
                .Row(new {id = 2, name = "Uploaded"})
                .Row(new {id = 3, name = "Uploading"})
                .Row(new {id = 4, name = "Error"});
            
            Alter.Table("stored_files")
                .AddColumn("data_to_upload").AsBinary(500 * 1024 * 1024).Nullable()
                .AddColumn("uploading_error").AsString().Nullable()
                .AddColumn("status").AsInt16().NotNullable().WithDefaultValue(2)
                .ForeignKey("stored_file_statuses", "id");
            
            base.Up();
        }
    }
}
