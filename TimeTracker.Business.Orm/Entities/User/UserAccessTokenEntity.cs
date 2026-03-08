using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.User
{
    public class UserAccessTokenEntity: AEntity
    {   
        public virtual required string Token { get; set; }
        public virtual DateTime ExpirationTime { get; set; }

        #region Relationships

        public virtual required UserEntity User { get; set; }
        public virtual ICollection<UserJwtTokenEntity> JwtTokens { get; set; } = new List<UserJwtTokenEntity>();

        #endregion
        
        #region Calculated

        public virtual bool IsExpired => ExpirationTime < DateTime.UtcNow;

        #endregion
    }
}
