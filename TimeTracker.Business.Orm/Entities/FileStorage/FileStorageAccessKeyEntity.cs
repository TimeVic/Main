using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    [Class(Table = "fs_access_keys")]
    public class FileStorageAccessKeyEntity: AEntity
    {        
        [Property(NotNull = true)]
        [Column(Name = "access_key", Length = 100, NotNull = false)]
        public virtual required string AccessKey { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "secret_key", Length = 100, NotNull = false)]
        public virtual required string SecretKey { get; set; }
        
        [Property(NotNull = false, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "expiration_time", SqlType = "datetime", NotNull = false)]
        public virtual DateTime? ExpirationTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual required UserEntity User { get; set; }
    }
}
