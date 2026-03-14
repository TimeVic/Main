using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingCounterEntity: AEntity
    {
        public virtual required long Counter { get; set; }

        #region Relationships

        public virtual required MessagingChannelEntity Channel { get; set; }
        public virtual required UserEntity User { get; set; }
        
        #endregion
    }
}
