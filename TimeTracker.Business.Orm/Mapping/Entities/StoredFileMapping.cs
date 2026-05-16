using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class StoredFileMapping: BaseGuidMappings<StoredFileEntity>
{
    public StoredFileMapping()
    {
        Table("stored_files");
        
        Map(x => x.Type).Enum<StoredFileType>();
        Map(x => x.CloudFilePath);
        Map(x => x.ThumbCloudFilePath).Nullable();
        Map(x => x.Extension).Nullable();
        Map(x => x.MimeType);
        Map(x => x.OriginalFileName);
        Map(x => x.Title).Nullable();
        Map(x => x.Description).Nullable();
        Map(x => x.Size).Nullable();
        
        Map(x => x.CreatedAt).DateTime();
        
        HasManyToMany(x => x.Tasks)
            .Table("task_stored_files")
            .ParentKeyColumn("stored_file_id")
            .ChildKeyColumn("task_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasManyToMany(x => x.TaskComments)
            .Table("task_comment_stored_files")
            .ParentKeyColumn("stored_file_id")
            .ChildKeyColumn("comment_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
        
        HasManyToMany(x => x.Users)
            .Table("user_stored_files")
            .ParentKeyColumn("stored_file_id")
            .ChildKeyColumn("user_id")
            .FetchType.Select()
            .LazyLoad()
            .Cascade.None();
    }
}
