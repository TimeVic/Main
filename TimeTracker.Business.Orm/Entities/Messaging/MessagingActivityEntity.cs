using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingActivityEntity: AEntity
    {
        public virtual required bool IsWriting { get; set; }
        public virtual required DateTime? WritingStartedAt { get; set; }

        #region Relationships

        public virtual required MessagingChannelEntity Channel { get; set; }
        public virtual required UserEntity User { get; set; }
        
        #endregion
    }
}
