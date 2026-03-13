using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.FileStorage
{
    public class FileStorageAccessKeyEntity: AEntity
    {        
        public virtual required string AccessKey { get; set; }
        public virtual required string SecretKey { get; set; }
        public virtual DateTime? ExpirationTime { get; set; }
 
        public virtual required UserEntity User { get; set; }
    }
}
