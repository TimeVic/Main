using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(49)]
    public class _49_Create_FileStorage : MyMigration
    {
        public override void Up()
        {
            Create.Table("fs_access_keys")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("access_key").AsString(100).NotNullable().Unique()
                .WithColumn("secret_key").AsString(100).NotNullable()
                .WithColumn("expiration_time").AsCustom("timestamptz").Nullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("fs_buckets")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id")
                .WithColumn("name").AsString(100).NotNullable().Unique()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("fs_directories")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("bucket_id").AsInt64().NotNullable().ForeignKey("fs_buckets", "id")
                .WithColumn("parent_id").AsInt64().Nullable().ForeignKey("fs_directories", "id")
                .WithColumn("name").AsString(1024).NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            Create.Table("fs_files")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("bucket_id").AsInt64().NotNullable().ForeignKey("fs_buckets", "id")
                .WithColumn("directory_id").AsInt64().Nullable().ForeignKey("fs_directories", "id")
                .WithColumn("external_id").AsString(50).NotNullable()
                .WithColumn("mongo_id").AsString(50).NotNullable()
                .WithColumn("name").AsString(1024).NotNullable()
                .WithColumn("extension").AsString(10).Nullable()
                .WithColumn("mime_type").AsString(30).NotNullable()
                .WithColumn("original_file_name").AsString(1024).NotNullable()
                .WithColumn("title").AsString(1024).Nullable()
                .WithColumn("description").AsString(1024).Nullable()
                .WithColumn("size").AsInt64().NotNullable()
                .WithColumn("update_time").AsCustom("timestamptz").NotNullable()
                .WithColumn("create_time").AsCustom("timestamptz").NotNullable();
            
            base.Up();
        }
    }
}
