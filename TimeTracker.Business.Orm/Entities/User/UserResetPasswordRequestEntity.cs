using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.User
{
    public class UserResetPasswordRequestEntity: AEntity
    {
        public virtual required string VerificationToken { get; set; }
        public virtual DateTime ExpirationTime { get; set; }

        #region Relationships

        public virtual required UserEntity User { get; set; }

        #endregion
        
        #region Calculated

        public virtual bool IsExpired
        {
            get => ExpirationTime < DateTime.UtcNow;
        }
        
        #endregion
    }
}
