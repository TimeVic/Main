using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    [Class(Table = "fs_files")]
    public class FileStorageFileEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "external_id", Length = 50, NotNull = false)]
        public virtual string ExternalId { get; set; } = string.Empty;
        
        [Property(NotNull = false)]
        [Column(Name = "mongo_id", Length = 50, NotNull = false)]
        public virtual string MongoId { get; set; } = string.Empty;
        
        [Property(NotNull = false)]
        [Column(Name = "name", Length = 1024, NotNull = false)]
        public virtual string Name { get; set; } = string.Empty;
        
        [Property(NotNull = false)]
        [Column(Name = "extension", Length = 10, NotNull = false)]
        public virtual string? Extension { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "mime_type", Length = 30, NotNull = true)]
        public virtual string MimeType { get; set; } = string.Empty;

        [Property(NotNull = true)]
        [Column(Name = "original_file_name", Length = 1024, NotNull = true)]
        public virtual string OriginalFileName { get; set; } = string.Empty;
        
        [Property(NotNull = false)]
        [Column(Name = "title", Length = 1024, NotNull = false)]
        public virtual string? Title { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "description", Length = 1024, NotNull = false)]
        public virtual string? Description { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "size", NotNull = false)]
        public virtual long Size { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(FileStorageBucketEntity), 
            Column = "bucket_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual FileStorageBucketEntity? Bucket { get; set; }
        
        #region Calculated

        public virtual string InternalFilePath
        {
            get
            {
                var filePath = Id.ToString();
                if (!string.IsNullOrEmpty(Extension))
                {
                    filePath += $".{Extension}";
                }

                return filePath;
            }
        }

        #endregion
    }
}
