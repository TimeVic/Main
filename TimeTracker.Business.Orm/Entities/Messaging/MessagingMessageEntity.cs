using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingMessageEntity: AEntity
    {
        public virtual required string Text { get; set; }
        
        #region Relationships

        public virtual required MessagingChannelEntity Channel { get; set; }
        public virtual required UserEntity CreatedBy { get; set; }

        #endregion
    }
}
