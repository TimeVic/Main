using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    [Class(Table = "fs_access_keys")]
    public class FileStorageAccessKeyEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "access_key", Length = 100, NotNull = false)]
        public virtual required string AccessKey { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "secret_key", Length = 100, NotNull = false)]
        public virtual required string SecretKey { get; set; }
        
        [Property(NotNull = false, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "expiration_time", SqlType = "datetime", NotNull = false)]
        public virtual DateTime? ExpirationTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual required DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual required DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual required UserEntity User { get; set; }
    }
}
