using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingChannelMemberEntity: AEntity
    {
        public virtual DateTime? DeactivatedAt { get; set; }
        
        #region Relationships

        public virtual required MessagingChannelEntity Channel { get; set; }
        public virtual required UserEntity Member { get; set; }

        #endregion
    }
}
