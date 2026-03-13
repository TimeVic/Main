using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.User
{
    public class UserJwtTokenEntity: AEntity
    {
        public virtual required string Token { get; set; }
        public virtual DateTime ExpirationTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserAccessTokenEntity), 
            Column = "access_token_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual required UserAccessTokenEntity AccessToken { get; set; }
        
        #region Calculated

        public virtual bool IsExpired => ExpirationTime < DateTime.UtcNow;

        #endregion
    }
}
