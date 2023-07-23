using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "user_reset_password_requests")]
    public class UserResetPasswordRequestEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "verification_token", Length = 200, NotNull = false)]
        public virtual string VerificationToken { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "expiration_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime ExpirationTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        #region Calculated

        public virtual bool IsExpired
        {
            get => ExpirationTime < DateTime.UtcNow;
        }
        
        #endregion
    }
}
