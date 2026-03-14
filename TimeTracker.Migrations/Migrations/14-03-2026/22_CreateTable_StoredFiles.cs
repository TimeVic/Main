using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(22)]
    public class _22_CreateTable_StoredFiles : MyMigration
    {
        public override void Up()
        {
            Create.Table("stored_file_types")
                .WithColumn("id").AsInt16().PrimaryKey()
                .WithColumn("name").AsString("").NotNullable();

            Insert.IntoTable("stored_file_types")
                .Row(new {id = 1, name = "Image"})
                .Row(new {id = 2, name = "Attachment"})
                .Row(new {id = 3, name = "Avatar"});
            
            Create.Table("stored_files")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("type").AsInt16().NotNullable()
                .ForeignKey("stored_file_types", "id")
                .WithColumn("cloud_file_path").AsString(512).NotNullable()
                .WithColumn("extension").AsString(20).Nullable()
                .WithColumn("mime_type").AsString(50).NotNullable()
                .WithColumn("size").AsInt64().Nullable()
                .WithColumn("original_file_name").AsString(100).NotNullable()
                .WithColumn("title").AsString(50).Nullable()
                .WithColumn("description").AsString(1024).Nullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();

            base.Up();
        }
    }
}
