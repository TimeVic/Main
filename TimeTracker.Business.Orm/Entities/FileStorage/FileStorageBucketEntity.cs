using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    [Class(Table = "fs_buckets")]
    public class FileStorageBucketEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "name", Length = 100, NotNull = false)]
        public virtual string Name { get; set; } = string.Empty;
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity? User { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "bucket_id")]
        [OneToMany(ClassType = typeof(FileStorageDirectoryEntity))]
        public virtual ICollection<FileStorageDirectoryEntity> Directories { get; set; } = new List<FileStorageDirectoryEntity>();
        
        #region Calculated

        public virtual ICollection<FileStorageDirectoryEntity> DirectoriesTree
        {
            get
            {
                return Directories.Where(item => item.Parent == null).ToList();
            }
        }

        #endregion
    }
}
