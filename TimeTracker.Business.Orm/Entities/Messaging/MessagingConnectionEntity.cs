using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingConnectionEntity: AEntity
    {
        public virtual required string ConnectionId { get; set; }
        
        #region Relationships

        public virtual required UserEntity User { get; set; }

        #endregion
    }
}
