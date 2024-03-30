using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    [Class(Table = "fs_directories")]
    public class FileStorageDirectoryEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "name", Length = 1024, NotNull = false)]
        public virtual string Name { get; set; } = string.Empty;
        
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
        public virtual required FileStorageBucketEntity Bucket { get; set; }
        
        [ManyToOne(
            ClassType = typeof(FileStorageDirectoryEntity), 
            Column = "parent_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual FileStorageDirectoryEntity? Parent { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "parent_id")]
        [OneToMany(ClassType = typeof(FileStorageDirectoryEntity))]
        public virtual ICollection<FileStorageDirectoryEntity> Children { get; set; } = new List<FileStorageDirectoryEntity>();
        
        #region Calculated

        public virtual string FullPath
        {
            get
            {
                var directories = BuildPathRecursive([Name], Parent);
                directories.Reverse();
                return string.Join('/', directories);
            }
        }

        private List<string> BuildPathRecursive(List<string> directories, FileStorageDirectoryEntity? parent)
        {
            if (parent == null)
            {
                return directories;
            }
            directories.Add(parent.Name);
            return BuildPathRecursive(directories, parent.Parent);
        }

        #endregion
    }
}
