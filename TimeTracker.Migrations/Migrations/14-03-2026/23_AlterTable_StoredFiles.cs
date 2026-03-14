using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(23)]
    public class _23_AlterTable_StoredFiles : MyMigration
    {
        public override void Up()
        {
            Alter.Table("stored_files")
                .AddColumn("thumb_cloud_file_path").AsString(512).Nullable();

            base.Up();
        }
    }
}
