using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities.User
{
    [Class(Table = "user_access_tokens")]
    public class UserAccessTokenEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "token", Length = 200, NotNull = true)]
        public virtual string Token { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "last_jwt", Length = 512, NotNull = true)]
        public virtual string LastJwt { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "expiration_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime ExpirationTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        #region Calculated

        public virtual bool IsExpired => ExpirationTime < DateTime.UtcNow;

        #endregion
    }
}
