using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.User
{
    public class UserNotificationTokenEntity: AEntity
    {   
        public virtual required string Token { get; set; }

        #region Relationships

        public virtual required UserEntity User { get; set; }

        #endregion
    }
}
