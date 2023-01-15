using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "stored_files")]
    public class StoredFileEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(StoredFileType))]
        [Column(Name = "type", NotNull = true)]
        public virtual StoredFileType Type { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "cloud_file_path", Length = 512, NotNull = true)]
        public virtual string CloudFilePath { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "thumb_cloud_file_path", Length = 512, NotNull = false)]
        public virtual string? ThumbCloudFilePath { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "extension", Length = 20, NotNull = false)]
        public virtual string? Extension { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "mime_type", Length = 50, NotNull = true)]
        public virtual string MimeType { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "original_file_name", Length = 100, NotNull = true)]
        public virtual string OriginalFileName { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "title", Length = 50, NotNull = false)]
        public virtual string? Title { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "description", Length = 1024, NotNull = false)]
        public virtual string? Description { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "size", NotNull = false)]
        public virtual long? Size { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
    }
}
